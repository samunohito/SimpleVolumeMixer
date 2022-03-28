using System.IO;
using System.Reflection;

using Microsoft.Extensions.Configuration;

using NUnit.Framework;

using Prism.Regions;

using SimpleVolumeMixer.Contracts.Services;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Services;
using SimpleVolumeMixer.Models;
using SimpleVolumeMixer.Services;
using SimpleVolumeMixer.ViewModels;

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

            // Core Services
            _container.RegisterType<IFileService, FileService>();

            // App Services
            _container.RegisterType<IThemeSelectorService, ThemeSelectorService>();
            _container.RegisterType<ISystemService, SystemService>();
            _container.RegisterType<IPersistAndRestoreService, PersistAndRestoreService>();
            _container.RegisterType<IApplicationInfoService, ApplicationInfoService>();

            // Configuration
            var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            var configuration = new ConfigurationBuilder()
                .SetBasePath(appLocation)
                .AddJsonFile("appsettings.json")
                .Build();
            var appConfig = configuration
                .GetSection(nameof(AppConfig))
                .Get<AppConfig>();

            // Register configurations to IoC
            _container.RegisterInstance(configuration);
            _container.RegisterInstance(appConfig);
        }

        // TODO WTS: Add tests for functionality you add to MainViewModel.
        [Test]
        public void TestMainViewModelCreation()
        {
            var vm = _container.Resolve<MainViewModel>();
            Assert.IsNotNull(vm);
        }

        // TODO WTS: Add tests for functionality you add to SettingsViewModel.
        [Test]
        public void TestSettingsViewModelCreation()
        {
            var vm = _container.Resolve<SettingsViewModel>();
            Assert.IsNotNull(vm);
        }
    }
}
