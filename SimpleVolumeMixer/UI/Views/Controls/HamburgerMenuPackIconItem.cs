using System.Windows;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;

namespace SimpleVolumeMixer.UI.Views.Controls;

public class HamburgerMenuPackIconItem : HamburgerMenuItem
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), 
        typeof(PackIconKind), 
        typeof(HamburgerMenuPackIconItem), 
        new PropertyMetadata(default(PackIconKind)));

    public PackIconKind Kind
    {
        get => (PackIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }
    
    protected override Freezable CreateInstanceCore()
    {
        return new HamburgerMenuPackIconItem();
    }
}