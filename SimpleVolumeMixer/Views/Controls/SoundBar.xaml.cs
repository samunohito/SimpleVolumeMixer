using System;
using System.Windows;
using System.Windows.Controls;

namespace SimpleVolumeMixer.Views.Controls
{
    /// <summary>
    /// SoundBar.xaml の相互作用ロジック
    /// </summary>
    public partial class SoundBar : UserControl
    {
        public static readonly DependencyProperty MeterMaximumProperty =
            DependencyProperty.Register(
                nameof(MeterMaximum),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(1.0, MeterMaximumPropertyChanged));

        private static readonly DependencyProperty MeterValueProperty =
            DependencyProperty.Register(
                nameof(MeterValue),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(0.0));

        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(0.0, ValuePropertyChanged));

        private static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(100.0, MaximumPropertyChanged));

        private static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(0.0, MinimumPropertyChanged));

        private static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(
                nameof(SmallChange),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(1.0, SmallChangePropertyChanged));

        private static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register(
                nameof(LargeChange),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(10.0, LargeChangePropertyChanged));

        private static readonly DependencyProperty HandlerProperty =
            DependencyProperty.Register(
                nameof(Handler),
                typeof(ISoundBarHandler),
                typeof(SoundBar),
                new PropertyMetadata(null));

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

        public ISoundBarHandler Handler
        {
            get => (ISoundBarHandler)GetValue(HandlerProperty);
            set => SetValue(HandlerProperty, value);
        }

        private static void MeterMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var soundBar = (SoundBar)d;
            soundBar.UpdateGreenAreaHeight(soundBar.MeterValue);
        }

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var soundBar = (SoundBar)d;
            soundBar.ValueSlider.Value = (double)e.NewValue;
        }

        private static void MaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var soundBar = (SoundBar)d;
            soundBar.ValueSlider.Maximum = (double)e.NewValue;
        }

        private static void MinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var soundBar = (SoundBar)d;
            soundBar.ValueSlider.Minimum = (double)e.NewValue;
        }

        private static void SmallChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var soundBar = (SoundBar)d;
            soundBar.ValueSlider.SmallChange = (double)e.NewValue;
        }

        private static void LargeChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var soundBar = (SoundBar)d;
            soundBar.ValueSlider.LargeChange = (double)e.NewValue;
        }

        public SoundBar()
        {
            InitializeComponent();

            var handler = new InternalSoundBarHandler();
            handler.NotifiedValue += OnNotifiedValue;

            handler.NotifyValue(0.0);
            Dispatcher.BeginInvoke((Action)(() => Handler = handler));
        }

        private void OnNotifiedValue(object? sender, SoundBarNotifyValueEventArgs e)
        {
            var value = e.Value;

            Dispatcher.Invoke(() =>
            {
                UpdateGreenAreaHeight(value);
                MeterValue = value;
            });
        }

        private void UpdateGreenAreaHeight(double value)
        {
            var canvasHeight = BackCanvas.ActualHeight;

            if (value <= 0.0)
            {
                GreenArea.Height = 0;
                return;
            }

            var maximum = MeterMaximum;
            if (maximum <= value)
            {
                GreenArea.Height = canvasHeight;
                return;
            }

            var newHeight = canvasHeight * (value / maximum);
            if (newHeight <= 0.0)
            {
                GreenArea.Height = 0;
                return;
            }

            if (canvasHeight <= newHeight)
            {
                GreenArea.Height = canvasHeight;
                return;
            }

            GreenArea.Height = newHeight;
        }
        
        private void ValueSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = e.NewValue;
        }

        private class InternalSoundBarHandler : ISoundBarHandler
        {
            public event EventHandler<SoundBarNotifyValueEventArgs>? NotifiedValue;

            public void NotifyValue(double value)
            {
                NotifiedValue?.Invoke(this, new SoundBarNotifyValueEventArgs(value));
            }
        }

        private class SoundBarNotifyValueEventArgs : EventArgs
        {
            public double Value { get; }

            public SoundBarNotifyValueEventArgs(double value)
            {
                Value = value;
            }
        }
    }
}