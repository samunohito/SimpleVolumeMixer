using System.IO;
using System.Reflection;

using Microsoft.Extensions.Configuration;

using NUnit.Framework;

using Prism.Regions;

using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Services;
using SimpleVolumeMixer.UI.Contracts.Services;
using SimpleVolumeMixer.UI.Models;
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

            // Core Services
            _container.RegisterType<IFileService, FileService>();

            // App Services
            _container.RegisterType<ISystemService, SystemService>();
            _container.RegisterType<IPersistAndRestoreService, PersistAndRestoreService>();
            _container.RegisterType<IApplicationInfoService, ApplicationInfoService>();

            // Configuration
            var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            var configuration = new ConfigurationBuilder()
                .SetBasePath(appLocation)
                .AddJsonFile(SimpleVolumeMixer.Properties.Resources.AppConfigFileName)
                .Build();
            var appConfig = configuration
                .GetSection(nameof(AppConfig))
                .Get<AppConfig>();

            // Register configurations to IoC
            _container.RegisterInstance(configuration);
            _container.RegisterInstance(appConfig);
        }

        [Test]
        public void TestAudioSessionsPageViewModelCreation()
        {
            var vm = _container.Resolve<AudioSessionsPageViewModel>();
            Assert.IsNotNull(vm);
        }
        
        [Test]
        public void TestAudioSessionsPageSubViewModel()
        {
            var vm = _container.Resolve<AudioSessionsPageSubViewModel>();
            Assert.IsNotNull(vm);
        }

        [Test]
        public void TestAudioDevicesPageViewModelCreation()
        {
            var vm = _container.Resolve<AudioDevicesPageViewModel>();
            Assert.IsNotNull(vm);
        }

        [Test]
        public void TestSettingsPageViewModelCreation()
        {
            var vm = _container.Resolve<SettingsPageViewModel>();
            Assert.IsNotNull(vm);
        }
    }
}
