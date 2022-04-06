using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.Component;


public class QueueProcessor<TP, TR> : DisposableComponent
{
    private readonly BlockingCollection<QueueProcessorHandle<TP, TR>> _handles;
    private readonly Task _loopTask;
    private readonly CancellationTokenSource _cancellation;

    public QueueProcessor(int capacity = 1024)
    {
        _handles = new BlockingCollection<QueueProcessorHandle<TP, TR>>(capacity).AddTo(Disposable);
        _loopTask = new Task(() => DoProcess()).AddTo(Disposable);
        _cancellation = new CancellationTokenSource().AddTo(Disposable);
    }

    public void StartRequest()
    {
        _loopTask.Start();
    }

    public void StopRequest()
    {
        _cancellation.Cancel(false);
    }

    public void Push(QueueProcessorHandle<TP, TR> handle)
    {
        _handles.Add(handle);
    }

    private void DoProcess()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            var handle = _handles.Take();
            if (handle.CancelRequest)
            {
                handle.Executed = true;
                continue;
            }
            
            handle.Result = handle.Function(handle.Argument);
            handle.Executed = true;
        }
    }

    protected override void OnDisposing()
    {
        StopRequest();
        base.OnDisposing();
    }
}
