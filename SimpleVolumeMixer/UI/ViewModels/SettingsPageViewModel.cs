using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using SimpleVolumeMixer.Properties;
using SimpleVolumeMixer.UI.Contracts.Services;
using SimpleVolumeMixer.UI.Models;

namespace SimpleVolumeMixer.UI.ViewModels;

public class SettingsPageViewModel : BindableBase, INavigationAware
{
    private readonly AppConfig _appConfig;
    private readonly IApplicationInfoService _applicationInfoService;
    private readonly ISystemService _systemService;
    private readonly IThemeSelectorService _themeSelectorService;

    public SettingsPageViewModel(
        AppConfig appConfig,
        IThemeSelectorService themeSelectorService,
        ISystemService systemService,
        IApplicationInfoService applicationInfoService
    )
    {
        _appConfig = appConfig;
        _themeSelectorService = themeSelectorService;
        _systemService = systemService;
        _applicationInfoService = applicationInfoService;

        VersionDescription = new ReactivePropertySlim<string?>();
        Theme = new ReactivePropertySlim<AppTheme?>();

        SetThemeCommand = new DelegateCommand<string>(OnSetTheme);
        GitHubStatementCommand = new DelegateCommand(OnGitHubStatement);
    }

    public IReactiveProperty<string?> VersionDescription { get; }
    public IReactiveProperty<AppTheme?> Theme { get; }

    public ICommand SetThemeCommand { get; }
    public ICommand GitHubStatementCommand { get; }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        VersionDescription.Value = $"{Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        Theme.Value = _themeSelectorService.GetCurrentTheme();
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

    private void OnGitHubStatement()
    {
        _systemService.OpenInWebBrowser(Resources.SettingsPageGitHubLink);
    }
}