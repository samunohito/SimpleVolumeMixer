using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using SimpleVolumeMixer.UI.Contracts.Services;

namespace SimpleVolumeMixer.UI.Services;

public partial class ToastNotificationsService : IToastNotificationsService
{
    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }
}