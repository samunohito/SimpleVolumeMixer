using System;

namespace SimpleVolumeMixer.Core.Helper.Component;

public static class QueueProcessorHandle
{
    public static QueueProcessorHandle<object?, object?> OfAction(Action action)
    {
        return OfAction(null, action);
    }

    public static QueueProcessorHandle<object?, object?> OfAction(string? identity, Action action)
    {
        return OfAction<object?>(identity, null, _ => { action(); });
    }

    public static QueueProcessorHandle<TA1, object?> OfAction<TA1>(TA1 args, Action<TA1> action)
    {
        return OfAction(null, args, action);
    }

    public static QueueProcessorHandle<TA1, object?> OfAction<TA1>(string? identity, TA1 args, Action<TA1> action)
    {
        return OfFunction<TA1, object?>(identity, args, x =>
        {
            action(x);
            return null;
        });
    }

    public static QueueProcessorHandle<object?, TR1> OfFunction<TR1>(Func<object?, TR1> func)
    {
        return OfFunction(null, func);
    }

    public static QueueProcessorHandle<object?, TR1> OfFunction<TR1>(string? identity, Func<object?, TR1> func)
    {
        return OfFunction(identity, null, func);
    }

    public static QueueProcessorHandle<TA1, TR1> OfFunction<TA1, TR1>(TA1 args, Func<TA1, TR1> func)
    {
        return OfFunction(null, args, func);
    }

    public static QueueProcessorHandle<TA1, TR1> OfFunction<TA1, TR1>(string? identity, TA1 args, Func<TA1, TR1> func)
    {
        return new QueueProcessorHandle<TA1, TR1>(identity, args, func);
    }
}

public class QueueProcessorHandle<TA, TR>
{
    public QueueProcessorHandle(string? identity, TA argument, Func<TA, TR> func)
    {
        Identity = identity ?? Guid.NewGuid().ToString();
        Function = func;
        Argument = argument;
        CancelRequest = false;

        Result = default;
    }

    public string Identity { get; }
    public Func<TA, TR> Function { get; }
    public TA Argument { get; }
    public TR? Result { get; set; }
    public bool CancelRequest { get; set; }
    public bool Executed { get; set; }
}