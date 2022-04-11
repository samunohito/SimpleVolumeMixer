using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NUnit.Framework;
using Prism.Regions;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Models.Repository;
using SimpleVolumeMixer.Core.Services;
using SimpleVolumeMixer.Properties;
using SimpleVolumeMixer.UI.Contracts.Services;
using SimpleVolumeMixer.UI.Helpers.Components;
using SimpleVolumeMixer.UI.Models;
using SimpleVolumeMixer.UI.Models.UseCase;
using SimpleVolumeMixer.UI.Services;
using SimpleVolumeMixer.UI.ViewModels;
using Unity;

namespace SimpleVolumeMixer.Tests.NUnit
{
    public class PagesTests
    {
        private IUnityContainer _container;

        [SetUp]
        public void Setup()
        {
            _container = new UnityContainer();
            _container.RegisterType<IRegionManager, RegionManager>();

            // Core Repositories
            _container.RegisterSingleton<ICoreAudioRepository, CoreAudioRepository>();

            // Core Services
            _container.RegisterType<IFileService, FileService>();
            _container.RegisterSingleton<ICoreAudioService, CoreAudioService>();

            // App Services
            _container.RegisterType<ISystemService, SystemService>();
            _container.RegisterType<IPersistAndRestoreService, PersistAndRestoreService>();
            _container.RegisterType<IApplicationInfoService, ApplicationInfoService>();
            
            // UseCases
            _container.RegisterType<AudioSessionsPageUseCase>(new DisposableComponentLifetimeManager());

            // ViewModels
            _container.RegisterType<SettingsPageViewModel>();
            _container.RegisterType<AudioSessionsPageViewModel>();
            _container.RegisterType<AudioSessionsPageSubViewModel>();
            _container.RegisterType<AudioSessionsPageSubViewModel>();
            _container.RegisterType<AudioDevicesPageViewModel>();

            // Configuration
            var appLocation = (Environment.CurrentDirectory);
            var configuration = new ConfigurationBuilder()
                .SetBasePath(appLocation)
                .AddJsonFile(Resources.AppConfigFileName)
                .Build();
            var appConfig = configuration
                .GetSection(nameof(AppConfig))
                .Get<AppConfig>();

            // Register configurations to IoC
            _container.RegisterInstance(configuration);
            _container.RegisterInstance(appConfig);

            // Logger
            var loggerFactory = new NLogLoggerFactory();
            var defaultLogger = loggerFactory.CreateLogger("default");
            _container.RegisterInstance<ILoggerFactory>(loggerFactory);
            _container.RegisterInstance(defaultLogger);
        }
        
        // TODO:fix

        // [Test]
        // public void TestAudioSessionsPageViewModelCreation()
        // {
        //     var vm = _container.Resolve<AudioSessionsPageViewModel>();
        //     Assert.IsNotNull(vm);
        // }
        //
        // [Test]
        // public void TestAudioSessionsPageSubViewModel()
        // {
        //     var vm = _container.Resolve<AudioSessionsPageSubViewModel>();
        //     Assert.IsNotNull(vm);
        // }
        //
        // [Test]
        // public void TestAudioDevicesPageViewModelCreation()
        // {
        //     var vm = _container.Resolve<AudioDevicesPageViewModel>();
        //     Assert.IsNotNull(vm);
        // }
        //
        // [Test]
        // public void TestSettingsPageViewModelCreation()
        // {
        //     var vm = _container.Resolve<SettingsPageViewModel>();
        //     Assert.IsNotNull(vm);
        // }
    }
}