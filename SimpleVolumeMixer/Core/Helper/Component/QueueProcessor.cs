using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DisposableComponents;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.Component;

/// <summary>
/// Provides a mechanism for managing <see cref="Func{T,TResult}"/> (and related objects) in a queue and executing them sequentially in a worker thread.
/// </summary>
/// <typeparam name="TP">Types of objects passed as Func arguments</typeparam>
/// <typeparam name="TR">Type of object returned as the return value of Func</typeparam>
public class QueueProcessor<TP, TR> : DisposableComponent
{
    private readonly string _name;
    private readonly ILogger _logger;
    private readonly BlockingCollection<QueueProcessorItem<TP, TR>> _items;
    private readonly Task _loopTask;
    private readonly CancellationTokenSource _cancellation;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="logger"></param>
    /// <param name="capacity">Maximum number of queues registered</param>
    public QueueProcessor(string name, ILogger logger, int capacity = 4096)
    {
        _name = name;
        _logger = logger;
        _items = new BlockingCollection<QueueProcessorItem<TP, TR>>(capacity).AddTo(Disposable);
        _cancellation = new CancellationTokenSource().AddTo(Disposable);
        _loopTask = new Task(() => DoProcess(), _cancellation.Token).AddTo(Disposable);
        _loopTask.Start();
    }

    /// <summary>
    /// Register Func (and its auxiliary objects) in the queue.
    /// When the number of registrations in the queue reaches the capacity set in the constructor,
    /// the thread calling this method blocks until the number of registrations in the queue is less than the capacity.
    /// </summary>
    /// <param name="item"></param>
    public void Push(QueueProcessorItem<TP, TR> item)
    {
        if (IsDisposed)
        {
            return;
        }
        
        _items.Add(item);
    }

    /// <summary>
    /// Infinite loop process called by worker thread
    /// </summary>
    private void DoProcess()
    {
        _logger.LogInformation("[{}] start looping...", _name);
        
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                var item = _items.Take(_cancellation.Token);
                if (item.CancelRequest)
                {
                    item.Executed = true;
                    continue;
                }

                item.Result = item.Function(item.Argument);
                item.Executed = true;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "[{}] canceled", _name);
            }
        }
        
        _logger.LogInformation("[{}] finish looping...", _name);
    }

    /// <inheritdoc cref="DisposableComponent.OnDisposing"/>
    protected override void OnDisposing()
    {
        _logger.LogInformation("[{}] disposing...", _name);
        
        _cancellation.Cancel(false);

        switch (_loopTask.Status)
        {
            case TaskStatus.Canceled:
            case TaskStatus.Faulted:
            case TaskStatus.RanToCompletion:
                break;
            default:
                _loopTask.Wait();
                break;
        }

        base.OnDisposing();
    }

    protected override void OnDisposed()
    {
        _logger.LogInformation("[{}] disposed...", _name);
        base.OnDisposed();
    }
}