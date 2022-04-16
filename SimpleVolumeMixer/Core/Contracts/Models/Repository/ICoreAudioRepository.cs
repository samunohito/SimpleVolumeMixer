using System.Collections.ObjectModel;
using Reactive.Bindings;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Contracts.Models.Repository;

public interface ICoreAudioRepository
{
    /// <summary>
    /// 現在使用可能な<see cref="AudioDeviceAccessor"/>の一覧を取得する。
    /// 読み取り専用であり、このオブジェクトからデバイスの増減を行うことは出来ない。
    /// デバイスの増減は<see cref="AudioDeviceAccessorManager.CollectAudioEndpoints"/>によりCoreAudioAPIからデバイス一覧を取り直すか、
    /// <see cref="AudioDeviceAccessorManager"/>がCoreAudioAPIからの通知を受け、その結果デバイスが追加されるかに限る。
    /// </summary>
    ReadOnlyObservableCollection<AudioDeviceAccessor> AudioDevices { get; }

    /// <summary>
    /// <see cref="RoleType.Communications"/>ロールのデバイスを取得する。
    /// インスタンスは<see cref="AudioDevices"/>から<see cref="RoleType.Communications"/>ロールを持つ物を検索して取得できる値と同一である。
    /// </summary>
    IReadOnlyReactiveProperty<AudioDeviceAccessor?> CommunicationRoleDevice { get; }

    /// <summary>
    /// <see cref="RoleType.Multimedia"/>ロールのデバイスを取得する。
    /// インスタンスは<see cref="AudioDevices"/>から<see cref="RoleType.Multimedia"/>ロールを持つ物を検索して取得できる値と同一である。
    /// </summary>
    IReadOnlyReactiveProperty<AudioDeviceAccessor?> MultimediaRoleDevice { get; }

    /// <summary>
    /// 引数のデバイスと<see cref="DataFlowType"/>のデバイスに対し、<see cref="RoleType"/>のロールを割り当てる。
    /// </summary>
    /// <param name="accessor"></param>
    /// <param name="dataFlowType"></param>
    /// <param name="roleType"></param>
    void SetDefaultDevice(AudioDeviceAccessor accessor, DataFlowType dataFlowType, RoleType roleType);
}