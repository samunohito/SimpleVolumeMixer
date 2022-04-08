
using System;
using SimpleVolumeMixer.Core.Helper.Component;
using Unity.Lifetime;

namespace SimpleVolumeMixer.UI.Helpers.Components;

public class DisposableComponentLifetimeManager : ContainerControlledLifetimeManager
{
    private IDisposableComponent? _disposableComponent = null;
    
    protected override LifetimeManager OnCreateLifetimeManager()
    {
        return new DisposableComponentLifetimeManager();
    }

    public override void SetValue(object newValue, ILifetimeContainer? container = null)
    {
        if (Equals(_disposableComponent, newValue))
        {
            return;
        }
        
        if (newValue is IDisposableComponent dc)
        {
            _disposableComponent = dc;
            dc.Disposed += OnTargetDisposed;
        }
        
        base.SetValue(newValue, container);
    }
    
    private void OnTargetDisposed(object? sender, EventArgs e)
    {
        if (_disposableComponent != null)
        {
            _disposableComponent.Disposed -= OnTargetDisposed;
            RemoveValue();
        }
    }
}