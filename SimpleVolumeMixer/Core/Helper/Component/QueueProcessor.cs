using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.Component;

/// <summary>
/// Provides a mechanism for managing <see cref="Func{T,TResult}"/> (and related objects) in a queue and executing them sequentially in a worker thread.
/// </summary>
/// <typeparam name="TP">Types of objects passed as Func arguments</typeparam>
/// <typeparam name="TR">Type of object returned as the return value of Func</typeparam>
public class QueueProcessor<TP, TR> : DisposableComponent
{
    private readonly BlockingCollection<QueueProcessorItem<TP, TR>> _items;
    private readonly Task _loopTask;
    private readonly CancellationTokenSource _cancellation;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="capacity">Maximum number of queues registered</param>
    public QueueProcessor(int capacity = 4096)
    {
        _items = new BlockingCollection<QueueProcessorItem<TP, TR>>(capacity).AddTo(Disposable);
        _loopTask = new Task(() => DoProcess()).AddTo(Disposable);
        _cancellation = new CancellationTokenSource().AddTo(Disposable);
    }

    /// <summary>
    /// Starts a worker thread that executes the Func registered in the Queue.
    /// The thread that invokes this method will not be blocked.
    /// </summary>
    public void StartRequest()
    {
        _loopTask.Start();
    }

    /// <summary>
    /// Sends a stop request to the worker thread.
    /// Note that this method call does not necessarily stop the worker thread immediately.
    /// </summary>
    public void StopRequest()
    {
        _cancellation.Cancel(false);
    }

    /// <summary>
    /// Register Func (and its auxiliary objects) in the queue.
    /// When the number of registrations in the queue reaches the capacity set in the constructor,
    /// the thread calling this method blocks until the number of registrations in the queue is less than the capacity.
    /// </summary>
    /// <param name="item"></param>
    public void Push(QueueProcessorItem<TP, TR> item)
    {
        _items.Add(item);
    }

    /// <summary>
    /// Infinite loop process called by worker thread
    /// </summary>
    private void DoProcess()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            var item = _items.Take();
            if (item.CancelRequest)
            {
                item.Executed = true;
                continue;
            }

            item.Result = item.Function(item.Argument);
            item.Executed = true;
        }
    }

    /// <inheritdoc cref="DisposableComponent.OnDisposing"/>
    protected override void OnDisposing()
    {
        StopRequest();
        base.OnDisposing();
    }
}