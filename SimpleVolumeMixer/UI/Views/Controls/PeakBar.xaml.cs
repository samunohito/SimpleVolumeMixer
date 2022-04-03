using System;
using System.Windows;
using System.Windows.Controls;

namespace SimpleVolumeMixer.UI.Views.Controls;

/// <summary>
///     SoundBar.xaml の相互作用ロジック
/// </summary>
public partial class PeakBar : UserControl
{
    public static readonly DependencyProperty MeterMaximumProperty =
        DependencyProperty.Register(
            nameof(MeterMaximum),
            typeof(double),
            typeof(PeakBar),
            new PropertyMetadata(1.0, MeterMaximumPropertyChanged));

    private static readonly DependencyProperty MeterValueProperty =
        DependencyProperty.Register(
            nameof(MeterValue),
            typeof(double),
            typeof(PeakBar),
            new PropertyMetadata(0.0));

    private static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(PeakBar),
            new PropertyMetadata(0.0));

    private static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(PeakBar),
            new PropertyMetadata(100.0));

    private static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(PeakBar),
            new PropertyMetadata(0.0));

    private static readonly DependencyProperty SmallChangeProperty =
        DependencyProperty.Register(
            nameof(SmallChange),
            typeof(double),
            typeof(PeakBar),
            new PropertyMetadata(1.0));

    private static readonly DependencyProperty LargeChangeProperty =
        DependencyProperty.Register(
            nameof(LargeChange),
            typeof(double),
            typeof(PeakBar),
            new PropertyMetadata(10.0));

    private readonly InternalSoundBarHandler _handler;

    public PeakBar()
    {
        InitializeComponent();

        _handler = new InternalSoundBarHandler();
        _handler.NotifiedValue += OnNotifiedValue;
        _handler.NotifyValue(0.0);
    }

    public double MeterMaximum
    {
        get => (double)GetValue(MeterMaximumProperty);
        set => SetValue(MeterMaximumProperty, value);
    }

    public double MeterValue
    {
        get => (double)GetValue(MeterValueProperty);
        set => SetValue(MeterValueProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double SmallChange
    {
        get => (double)GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public double LargeChange
    {
        get => (double)GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    public event EventHandler<SoundBarReadyEventArgs>? Ready
    {
        add { value?.Invoke(this, new SoundBarReadyEventArgs(_handler)); }
        // ReSharper disable once ValueParameterNotUsed
        remove { }
    }

    private static void MeterMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var soundBar = (PeakBar)d;
        soundBar.UpdateMeterArea(soundBar.MeterValue);
    }

    private void OnNotifiedValue(object? sender, SoundBarNotifyValueEventArgs e)
    {
        var value = e.Value;

        Dispatcher.Invoke(() =>
        {
            UpdateMeterArea(value);
            MeterValue = value;
        });
    }

    private void UpdateMeterArea(double newMeterValue)
    {
        var canvasHeight = BackCanvas.ActualHeight;

        if (newMeterValue <= 0.0)
        {
            GrayArea.Height = 0;
            GreenArea.Height = 0;
            return;
        }

        var barRatio = Maximum == 0.0 ? 1.0 : Value / Maximum;
        var meterMaximum = MeterMaximum;
        if (meterMaximum <= newMeterValue)
        {
            GrayArea.Height = canvasHeight;
            GreenArea.Height = canvasHeight * barRatio;
            return;
        }

        var newHeight = canvasHeight * (newMeterValue / meterMaximum);
        if (newHeight <= 0.0)
        {
            GrayArea.Height = 0;
            GreenArea.Height = 0;
            return;
        }

        if (canvasHeight <= newHeight)
        {
            GrayArea.Height = canvasHeight;
            GreenArea.Height = canvasHeight * barRatio;
            return;
        }

        GrayArea.Height = newHeight;
        GreenArea.Height = newHeight * barRatio;
    }

    private class InternalSoundBarHandler : ISoundBarHandler
    {
        public void NotifyValue(double value)
        {
            NotifiedValue?.Invoke(this, new SoundBarNotifyValueEventArgs(value));
        }

        public event EventHandler<SoundBarNotifyValueEventArgs>? NotifiedValue;
    }

    private class SoundBarNotifyValueEventArgs : EventArgs
    {
        public SoundBarNotifyValueEventArgs(double value)
        {
            Value = value;
        }

        public double Value { get; }
    }
}