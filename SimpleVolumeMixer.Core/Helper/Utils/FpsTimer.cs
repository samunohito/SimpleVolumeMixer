using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleVolumeMixer.Core.Helper.Utils;

public class FpsTimer
{
    public event EventHandler<EventArgs>? Tick;

    /// <summary>
    /// フレーム時刻の基準となる時刻（単位：ms）
    /// </summary>
    private readonly int _baseTickCount;

    /// <summary>
    /// 前回のフレームの時刻（単位：µs）
    /// </summary>
    private int _prevTickCount;

    /// <summary>
    /// 目標fps（保持用）
    /// </summary>
    private int _expectFps;

    /// <summary>
    /// 1フレームの時間（単位：μs）
    /// 60fpsの場合、60fps≒1.66μsとなる
    /// </summary>
    private int _period;

    /// <summary>
    /// fps計測用のカウンタ値
    /// </summary>
    private int _fpsCount;

    /// <summary>
    /// fps測定用の時刻（単位：ms）
    /// </summary>
    private int _fpsTickCount;

    /// <summary>
    /// fps制御を行うための無限ループを回すTask
    /// </summary>
    private Task? _timerTask;

    /// <summary>
    /// Taskの停止用トークン
    /// </summary>
    private CancellationTokenSource? _cancellation;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="fps">目標のfps（省略時は60fps）</param>
    public FpsTimer(int fps = 60)
    {
        _baseTickCount = Environment.TickCount;
        _timerTask = null;
        _cancellation = null;
        ExpectFps = fps;
    }

    /// <summary>
    /// 計測した（実際の）fps
    /// </summary>
    public int CurrentFps { get; private set; }

    /// <summary>
    /// 目標のfps
    /// </summary>
    public int ExpectFps
    {
        get => _expectFps;
        private set
        {
            _expectFps = value;
            _period = 1000 * 1000 / value;
        }
    }

    public void Start()
    {
        if (_timerTask != null && _cancellation != null)
        {
            return;
        }

        _cancellation = new CancellationTokenSource();
        _timerTask = new Task(TimerLoop, _cancellation.Token);
        _timerTask.Start();
    }

    public void Stop()
    {
        if (_timerTask == null || _cancellation == null)
        {
            return;
        }

        // ここで出来るのは要求のみ。破棄処理は無限ループを抜けた時にやる
        _cancellation.Cancel();
    }

    /// <summary>
    /// fps制御を行うための無限ループ
    /// </summary>
    private void TimerLoop()
    {
        if (_cancellation == null)
        {
            throw new ApplicationException("ありえないが、念の為");
        }

        while (!_cancellation.IsCancellationRequested)
        {
            WaitNextFrame();
            CalcFps();
            Tick?.Invoke(this, EventArgs.Empty);
        }

        _timerTask = null;
        _cancellation = null;
    }

    /// <summary>
    /// 次のフレームを待つ。ゲームループの先頭で呼び出すことを期待している。
    /// </summary>
    private void WaitNextFrame()
    {
        _prevTickCount += _period;

        var nowTickCount = (Environment.TickCount - _baseTickCount) * 1000;
        if (nowTickCount >= (_prevTickCount + _period))
        {
            return;
        }

        while (nowTickCount < (_prevTickCount + _period))
        {
            Thread.Sleep(1);
            nowTickCount = (Environment.TickCount - _baseTickCount) * 1000;
        }
    }

    /// <summary>
    /// fpsを計算する
    /// </summary>
    private void CalcFps()
    {
        // カウンタを増やす
        _fpsCount++;

        // 前回の計測時刻から1秒以上経過していれば、フレームレートを計算
        var tickCount = Environment.TickCount;
        if (tickCount - _fpsTickCount >= 1000)
        {
            CurrentFps = (_fpsCount * 1000) / (tickCount - _fpsTickCount);
            _fpsTickCount = tickCount;
            _fpsCount = 0;
        }
    }
}