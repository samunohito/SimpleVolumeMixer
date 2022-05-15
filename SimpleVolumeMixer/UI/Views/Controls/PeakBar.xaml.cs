using System;
using System.Diagnostics;
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

    private static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(PeakBar),
            new PropertyMetadata(Orientation.Vertical, OrientationPropertyChanged));

    private static readonly DependencyProperty PeakBarHandlerProperty =
        DependencyProperty.Register(
            nameof(PeakBarHandler),
            typeof(IPeakBarHandler),
            typeof(PeakBar),
            new PropertyMetadata(null, PeakBarHandlerPropertyChanged));

    private readonly InternalPeakBarHandler _handler;
    private readonly double _originWidth;
    private readonly double _originHeight;
    private IMeterSizeCalculator _calculator;

    public PeakBar()
    {
        InitializeComponent();

        _originWidth = 4.0;
        _originHeight = 4.0;

        _calculator = new VerticalMeterSizeCalculator(this);

        _handler = new InternalPeakBarHandler();
        _handler.NotifiedValue += OnNotifiedValue;

        PeakBarHandler = _handler;

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

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public IPeakBarHandler PeakBarHandler
    {
        get => (IPeakBarHandler)GetValue(PeakBarHandlerProperty);
        set => SetValue(PeakBarHandlerProperty, value);
    }

    public event EventHandler<PeakBarReadyEventArgs>? Ready
    {
        add { value?.Invoke(this, new PeakBarReadyEventArgs(_handler)); }
        // ReSharper disable once ValueParameterNotUsed
        remove { }
    }

    private static void MeterMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var peakBar = (PeakBar)d;
        peakBar.UpdateMeterSize(peakBar.MeterValue);
    }

    private static void PeakBarHandlerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var peakBar = (PeakBar)d;
        if (!Equals(peakBar.PeakBarHandler, peakBar._handler))
        {
            peakBar.PeakBarHandler = peakBar._handler;
        }
    }

    private static void OrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var peakBar = (PeakBar)d;
        var newValue = (Orientation)e.NewValue;
        switch (newValue)
        {
            case Orientation.Horizontal:
                peakBar._calculator = new HorizontalMeterSizeCalculator(peakBar);
                break;
            case Orientation.Vertical:
                peakBar._calculator = new VerticalMeterSizeCalculator(peakBar);
                break;
        }
    }

    private void OnNotifiedValue(object? sender, PeakBarNotifyValueEventArgs e)
    {
        var value = e.Value;

        Dispatcher.Invoke(() =>
        {
            UpdateMeterSize(value);
            MeterValue = value;
        });
    }

    private void UpdateMeterSize(double newMeterValue)
    {
        _calculator.UpdateMeterSize(newMeterValue);
    }

    private class InternalPeakBarHandler : IPeakBarHandler
    {
        public void NotifyValue(double value)
        {
            NotifiedValue?.Invoke(this, new PeakBarNotifyValueEventArgs(value));
        }

        public event EventHandler<PeakBarNotifyValueEventArgs>? NotifiedValue;
    }

    private class PeakBarNotifyValueEventArgs : EventArgs
    {
        public PeakBarNotifyValueEventArgs(double value)
        {
            Value = value;
        }

        public double Value { get; }
    }

    private interface IMeterSizeCalculator
    {
        /// <summary>
        /// メーター部分の描画領域サイズとして使用する値
        /// </summary>
        double MaximumCanvasSize { get; }

        /// <summary>
        /// メーター部分の最大値として使用する値
        /// </summary>
        double MaximumMeterValue { get; }

        /// <summary>
        /// スライダー部分の最大値として使用する値
        /// </summary>
        double MaximumSliderValue { get; }

        /// <summary>
        /// スライダー部分の現在値として使用する値
        /// </summary>
        double CurrentSliderValue { get; }

        /// <summary>
        /// メーターの描画サイズを計算し、サイズを更新する
        /// </summary>
        /// <param name="newMeterValue"></param>
        void UpdateMeterSize(double newMeterValue);
    }

    private abstract class AbstractMeterSizeCalculator : IMeterSizeCalculator
    {
        public AbstractMeterSizeCalculator(PeakBar parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// サイズ計算対象の<see cref="PeakBar"/>
        /// </summary>
        protected PeakBar Parent { get; }

        /// <inheritdoc cref="IMeterSizeCalculator.MaximumCanvasSize"/>
        public abstract double MaximumCanvasSize { get; }

        /// <inheritdoc cref="IMeterSizeCalculator.MaximumMeterValue"/>
        public double MaximumMeterValue => Parent.MeterMaximum;

        /// <inheritdoc cref="IMeterSizeCalculator.MaximumSliderValue"/>
        public double MaximumSliderValue => Parent.Maximum;

        /// <inheritdoc cref="IMeterSizeCalculator.CurrentSliderValue"/>
        public double CurrentSliderValue => Parent.Value;

        /// <inheritdoc cref="IMeterSizeCalculator.UpdateMeterSize"/>
        public void UpdateMeterSize(double newMeterValue)
        {
            var maximumSliderValue = MaximumSliderValue;

            if (newMeterValue <= 0.0 || maximumSliderValue == 0.0)
            {
                SetMeterSize(0);
                return;
            }

            var maximumCanvasSize = MaximumCanvasSize;
            var maximumMeterValue = MaximumMeterValue;
            if (maximumMeterValue <= newMeterValue)
            {
                SetMeterSize(maximumCanvasSize);
                return;
            }

            var newMeterSize = maximumCanvasSize * (newMeterValue / maximumMeterValue);
            if (newMeterSize <= 0.0)
            {
                SetMeterSize(0);
                return;
            }

            if (maximumCanvasSize <= newMeterSize)
            {
                SetMeterSize(newMeterSize);
                return;
            }

            SetMeterSize(newMeterSize);
        }

        protected double CalcSliderValueRatio()
        {
            return CurrentSliderValue / MaximumSliderValue;
        }

        /// <summary>
        /// 縦か横かにより更新するプロパティが異なるので抽象化
        /// </summary>
        /// <param name="newSize"></param>
        protected abstract void SetMeterSize(double newSize);
    }

    /// <summary>
    /// 横向き時に使用するメーター部分サイズ計算用クラス
    /// </summary>
    private class HorizontalMeterSizeCalculator : AbstractMeterSizeCalculator
    {
        public HorizontalMeterSizeCalculator(PeakBar parent) : base(parent)
        {
        }

        public override double MaximumCanvasSize => Parent.BackCanvas.ActualWidth;

        protected override void SetMeterSize(double newSize)
        {
            var grayArea = Parent.GrayArea;
            var greenArea = Parent.GreenArea;
            var originHeight = Parent._originHeight;
            var barRatio = CalcSliderValueRatio();

            grayArea.Width = newSize;
            greenArea.Width = newSize * barRatio;
            if (Math.Abs(grayArea.Height - originHeight) > 0.01)
            {
                grayArea.Height = originHeight;
                greenArea.Height = originHeight;
            }
        }
    }

    /// <summary>
    /// 縦向き時に使用するメーター部分サイズ計算用クラス
    /// </summary>
    private class VerticalMeterSizeCalculator : AbstractMeterSizeCalculator
    {
        public VerticalMeterSizeCalculator(PeakBar parent) : base(parent)
        {
        }

        public override double MaximumCanvasSize => Parent.BackCanvas.ActualHeight;

        protected override void SetMeterSize(double newSize)
        {
            var grayArea = Parent.GrayArea;
            var greenArea = Parent.GreenArea;
            var originWidth = Parent._originWidth;
            var barRatio = CalcSliderValueRatio();

            grayArea.Height = newSize;
            greenArea.Height = newSize * barRatio;
            if (Math.Abs(grayArea.Width - originWidth) > 0.01)
            {
                grayArea.Width = originWidth;
                greenArea.Width = originWidth;
            }
        }
    }
}