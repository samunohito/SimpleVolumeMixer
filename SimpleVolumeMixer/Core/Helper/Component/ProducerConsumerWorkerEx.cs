using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Reactive.Bindings.Extensions;
using SimpleComponents.ProducerConsumer;

namespace SimpleVolumeMixer.Core.Helper.Component;

public class ProducerConsumerWorkerEx : ProducerConsumerWorker
{
    private readonly CompositeDisposable _disposable;
    private readonly Subject<ProducerConsumerWorkerItem> _subject;

    public ProducerConsumerWorkerEx(IScheduler scheduler)
    {
        _disposable = new CompositeDisposable();
        _subject = new Subject<ProducerConsumerWorkerItem>();
        _subject
            .ObserveOn(scheduler)
            .Subscribe(item => base.DoExecute(item))
            .AddTo(_disposable);
    }

    protected override void DoExecute(ProducerConsumerWorkerItem item)
    {
        _subject.OnNext(item);
    }

    public new void Dispose()
    {
        base.Dispose();
        _disposable.Dispose();
    }
}