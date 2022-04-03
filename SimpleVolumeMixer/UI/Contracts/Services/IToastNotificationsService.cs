using Windows.UI.Notifications;

namespace SimpleVolumeMixer.UI.Contracts.Services;

public interface IToastNotificationsService
{
    public void ShowToastNotification(ToastNotification toastNotification);

    public void ShowToastNotificationSample();
}