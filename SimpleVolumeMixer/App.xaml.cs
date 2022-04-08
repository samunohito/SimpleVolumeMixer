using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using NLog.Extensions.Logging;
using Prism.Ioc;
using Prism.Unity;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Models.Repository;
using SimpleVolumeMixer.Core.Services;
using SimpleVolumeMixer.UI.Constants;
using SimpleVolumeMixer.UI.Contracts.Services;
using SimpleVolumeMixer.UI.Helpers.Components;
using SimpleVolumeMixer.UI.Models;
using SimpleVolumeMixer.UI.Models.UseCase;
using SimpleVolumeMixer.UI.Services;
using SimpleVolumeMixer.UI.ViewModels;
using SimpleVolumeMixer.UI.Views;
using SimpleVolumeMixer.UI.Views.Controls;
using Unity;

namespace SimpleVolumeMixer;
// For more inforation about application lifecyle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview
// For docs about using Prism in WPF see https://prismlibrary.com/docs/wpf/introduction.html

// WPF UI elements use language en-US by default.
// If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
// Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
public partial class App : PrismApplication
{
    private const string ToastNotificationActivationArguments = "ToastNotificationActivationArguments";

    private string[] _startUpArgs;

    protected override Window CreateShell()
    {
        return Container.Resolve<ShellWindow>();
    }

    protected override async void OnInitialized()
    {
        var persistAndRestoreService = Container.Resolve<IPersistAndRestoreService>();
        persistAndRestoreService.RestoreData();

        var themeSelectorService = Container.Resolve<IThemeSelectorService>();
        themeSelectorService.InitializeTheme();

        // https://docs.microsoft.com/windows/uwp/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            Current.Dispatcher.Invoke(async () =>
            {
                var config = Container.Resolve<IConfiguration>();

                // Store ToastNotification arguments in configuration, so you can use them from any point in the app
                config[ToastNotificationActivationArguments] = toastArgs.Argument;

                Current.MainWindow.Show();
                Current.MainWindow.Activate();
                if (Current.MainWindow.WindowState == WindowState.Minimized)
                    Current.MainWindow.WindowState = WindowState.Normal;

                await Task.CompletedTask;
            });
        };

        base.OnInitialized();
        await Task.CompletedTask;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        _startUpArgs = e.Args;
        base.OnStartup(e);
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Core Repositories
        containerRegistry.RegisterSingleton<ICoreAudioRepository, CoreAudioRepository>();

        // Core Services
        containerRegistry.Register<IFileService, FileService>();
        containerRegistry.RegisterSingleton<ICoreAudioService, CoreAudioService>();

        // App Services
        containerRegistry.RegisterSingleton<IToastNotificationsService, ToastNotificationsService>();
        containerRegistry.Register<IApplicationInfoService, ApplicationInfoService>();
        containerRegistry.Register<ISystemService, SystemService>();
        containerRegistry.Register<IPersistAndRestoreService, PersistAndRestoreService>();
        containerRegistry.Register<IThemeSelectorService, ThemeSelectorService>();
        containerRegistry.RegisterSingleton<IRightPaneService, RightPaneService>();
        
        // UseCases
        containerRegistry.GetContainer().RegisterType<AudioSessionsPageUseCase>(new DisposableComponentLifetimeManager());

        // Views
        containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>(PageKeys.Settings);
        containerRegistry.RegisterForNavigation<AudioSessionsPage, AudioSessionsPageViewModel>(PageKeys.AudioSessions);
        containerRegistry.RegisterForNavigation<AudioSessionsSubHorizontalPage, AudioSessionsPageSubViewModel>(PageKeys.AudioSessionsSubHorizontal);
        containerRegistry.RegisterForNavigation<AudioSessionsSubVerticalPage, AudioSessionsPageSubViewModel>(PageKeys.AudioSessionsSubVertical);
        containerRegistry.RegisterForNavigation<AudioDevicesPage, AudioDevicesPageViewModel>(PageKeys.AudioDevices);

        containerRegistry.RegisterForNavigation<ShellWindow, ShellWindowViewModel>();

        // Configuration
        var configuration = BuildConfiguration();
        var appConfig = configuration
            .GetSection(nameof(AppConfig))
            .Get<AppConfig>();

        // Register configurations to IoC
        containerRegistry.RegisterInstance(configuration);
        containerRegistry.RegisterInstance(appConfig);

        // Logger
        var loggerFactory = new NLogLoggerFactory();
        var defaultLogger = loggerFactory.CreateLogger("default");
        containerRegistry.RegisterInstance<ILoggerFactory>(loggerFactory);
        containerRegistry.RegisterInstance(defaultLogger);
    }

    private IConfiguration BuildConfiguration()
    {
        // TODO: Register arguments you want to use on App initialization
        var activationArgs = new Dictionary<string, string>
        {
            { ToastNotificationActivationArguments, string.Empty }
        };

        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        return new ConfigurationBuilder()
            .SetBasePath(appLocation)
            .AddJsonFile("appsettings.json")
            .AddCommandLine(_startUpArgs)
            .AddInMemoryCollection(activationArgs)
            .Build();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        var persistAndRestoreService = Container.Resolve<IPersistAndRestoreService>();
        persistAndRestoreService.PersistData();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // TODO WTS: Please log and handle the exception as appropriate to your scenario
        // For more info see https://docs.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception?view=netcore-3.0
    }
}