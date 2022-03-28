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
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(0.0, MaximumPropertyChanged));

        private static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register(
                nameof(CurrentValue),
                typeof(double),
                typeof(SoundBar),
                new PropertyMetadata(0.0));

        private static readonly DependencyProperty HandlerProperty =
            DependencyProperty.Register(
                nameof(Handler),
                typeof(ISoundBarHandler),
                typeof(SoundBar),
                new PropertyMetadata(null));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double CurrentValue
        {
            get => (double)GetValue(CurrentValueProperty);
            set => SetValue(CurrentValueProperty, value);
        }

        public ISoundBarHandler Handler
        {
            get => (ISoundBarHandler)GetValue(HandlerProperty);
            set => SetValue(HandlerProperty, value);
        }
        
        private static void MaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var soundBar = (SoundBar)d;
            soundBar.UpdateGreenAreaHeight(soundBar.CurrentValue);
        }

        public SoundBar()
        {
            InitializeComponent();

            var handler = new InternalSoundBarHandler();
            handler.NotifiedValue += OnNotifiedValue;

            Dispatcher.BeginInvoke((Action)(() => Handler = handler));
        }

        private void OnNotifiedValue(object? sender, SoundBarNotifyValueEventArgs e)
        {
            var value = e.Value;

            Dispatcher.Invoke(() =>
            {
                UpdateGreenAreaHeight(value);
                CurrentValue = value;
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

            var maximum = Maximum;
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