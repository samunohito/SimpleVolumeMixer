using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SimpleVolumeMixer.Properties;
using SimpleVolumeMixer.UI.Contracts.Services;
using SimpleVolumeMixer.UI.Models;

namespace SimpleVolumeMixer.UI.ViewModels;

// TODO WTS: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
public class SettingsViewModel : BindableBase, INavigationAware
{
    private readonly AppConfig _appConfig;
    private readonly IApplicationInfoService _applicationInfoService;
    private readonly ISystemService _systemService;
    private readonly IThemeSelectorService _themeSelectorService;
    private ICommand _privacyStatementCommand;
    private ICommand _setThemeCommand;
    private AppTheme _theme;
    private string _versionDescription;

    public SettingsViewModel(AppConfig appConfig, IThemeSelectorService themeSelectorService,
        ISystemService systemService, IApplicationInfoService applicationInfoService)
    {
        _appConfig = appConfig;
        _themeSelectorService = themeSelectorService;
        _systemService = systemService;
        _applicationInfoService = applicationInfoService;
    }

    public AppTheme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SetThemeCommand => _setThemeCommand ?? (_setThemeCommand = new DelegateCommand<string>(OnSetTheme));

    public ICommand PrivacyStatementCommand => _privacyStatementCommand ??
                                               (_privacyStatementCommand = new DelegateCommand(OnPrivacyStatement));

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        VersionDescription = $"{Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        Theme = _themeSelectorService.GetCurrentTheme();
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    private void OnSetTheme(string themeName)
    {
        var theme = (AppTheme)Enum.Parse(typeof(AppTheme), themeName);
        _themeSelectorService.SetTheme(theme);
    }

    private void OnPrivacyStatement()
    {
        _systemService.OpenInWebBrowser(_appConfig.PrivacyStatement);
    }
}