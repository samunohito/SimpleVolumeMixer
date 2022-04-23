using System;
using System.Threading;
using System.Threading.Tasks;
using CSCore.CoreAudioAPI;
using DisposableComponents;
using Microsoft.Extensions.Logging;
using SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionManagerAccessor : DisposableComponent
{
    public event EventHandler<EventArgs>? SessionManagerOpened;
    public event EventHandler<EventArgs>? SessionManagerClosed;
    public event EventHandler<SessionCreatedEventArgs>? SessionCreated;

    private readonly object _gate = new();
    private readonly ILogger _logger;
    private readonly AudioDeviceAccessor _device;
    private AudioSessionManagerEventAdapter? _eventAdapter;
    private AudioSessionManager2? _sessionManager;

    internal AudioSessionManagerAccessor(AudioDeviceAccessor device, ILogger logger)
    {
        _logger = logger;
        _device = device;
        _eventAdapter = null;
        _sessionManager = null;
    }

    public void OnSessionCreated(object? sender, SessionCreatedEventArgs e)
    {
        SessionCreated?.Invoke(this, e);
    }

    public async Task OpenSessionManager()
    {
        // 古いインスタンスがある場合は破棄
        CloseSessionManager();
        
        _logger.LogInformation("open sessionManager...");

        // AudioSessionManager2はMTAスレッドでしか取得できないので、別スレッドを起動する（WPFはSTAスレッド）
        var tcs = new TaskCompletionSource<AudioSessionManager2>();
        var thread = new Thread((args) =>
        {
            if (args is MMDevice device)
            {
                tcs.SetResult(AudioSessionManager2.FromMMDevice(device));
                return;
            }
        
            throw new InvalidOperationException("MMDevice以外が渡されてくるのは実装上あり得ない");
        });
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start(_device.Device);

        var sessionManager = await tcs.Task;

        lock (_gate)
        {
            // 個別で破棄するのでDisposableには入れない
            _sessionManager = sessionManager;
            _eventAdapter = new AudioSessionManagerEventAdapter(sessionManager, _logger);
            _eventAdapter.SessionCreated += OnSessionCreated;
        }

        SessionManagerOpened?.Invoke(this, EventArgs.Empty);
    }

    public void CloseSessionManager()
    {
        _logger.LogInformation("close sessionManager...");

        var sessionManager = _sessionManager;
        var eventAdapter = _eventAdapter;

        lock (_gate)
        {
            if (_sessionManager == null || _eventAdapter == null)
            {
                _logger.LogInformation("sessionManger is not allocated");
                return;
            }

            _sessionManager = null;

            _eventAdapter.SessionCreated -= OnSessionCreated;
            _eventAdapter = null;
        }

        // 破棄処理は排他する必要なく単独で動かしてもいいのでlockの外でやる
        sessionManager?.Dispose();
        eventAdapter?.Dispose();

        _logger.LogInformation("closed sessionManager : {}", sessionManager);

        SessionManagerClosed?.Invoke(this, EventArgs.Empty);
    }

    public AudioSessionEnumerator? GetEnumerator()
    {
        lock (_gate)
        {
            if (_sessionManager == null)
            {
                return null;
            }

            return _sessionManager.GetSessionEnumerator();
        }
    }

    protected override void OnDisposing()
    {
        _logger.LogInformation("disposing ...");

        CloseSessionManager();
        base.OnDisposing();
    }
}