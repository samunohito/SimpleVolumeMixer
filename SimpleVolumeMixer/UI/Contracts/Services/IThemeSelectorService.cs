using SimpleVolumeMixer.UI.Models;

namespace SimpleVolumeMixer.UI.Contracts.Services;

public interface IThemeSelectorService
{
    void InitializeTheme();

    void SetTheme(AppTheme theme);

    AppTheme GetCurrentTheme();
}