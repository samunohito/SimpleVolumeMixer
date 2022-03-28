using Microsoft.Toolkit.Uwp.Notifications;

using SimpleVolumeMixer.Contracts.Services;

using Windows.UI.Notifications;

namespace SimpleVolumeMixer.Services
{
    public partial class ToastNotificationsService : IToastNotificationsService
    {
        public ToastNotificationsService()
        {
        }

        public void ShowToastNotification(ToastNotification toastNotification)
        {
            ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
        }
    }
}
