using Reactive.Bindings;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Contracts.Services;

public interface ICoreAudioService
{
    ReadOnlyReactiveCollection<AudioDevice> RenderDevices { get; }
    ReadOnlyReactiveCollection<AudioDevice> CaptureDevices { get; }
    IReadOnlyReactiveProperty<AudioDevice?> CommunicationRoleRenderDevice { get; }
    IReadOnlyReactiveProperty<AudioDevice?> MultimediaRoleRenderDevice { get; }
    IReadOnlyReactiveProperty<AudioDevice?> CommunicationRoleCaptureDevice { get; }
    IReadOnlyReactiveProperty<AudioDevice?> MultimediaRoleCaptureDevice { get; }
    void SetDefaultDevice(AudioDevice device, DataFlowType dataFlowType, RoleType roleType);
}