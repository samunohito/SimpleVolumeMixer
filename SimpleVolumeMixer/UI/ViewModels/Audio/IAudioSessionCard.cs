using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Reactive.Bindings;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.ViewModels.Audio;

public interface IAudioSessionCard
{
    bool UsePackIcon => false;
    PackIconKind? PackIconKind => null;
    IReadOnlyReactiveProperty<string?> DisplayName { get; }
    IReadOnlyReactiveProperty<ImageSource?> IconSource { get; }
    IReactiveProperty<float> MasterVolume { get; }
    IReactiveProperty<bool> IsMuted { get; }
    IReadOnlyReactiveProperty<float> PeakValue { get; }
    ICommand PeakBarReadyCommand { get; }
    ICommand MuteStateChangeCommand { get; }
}