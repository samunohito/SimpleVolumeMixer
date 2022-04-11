using System;
using Moq;
using NUnit.Framework;
using SimpleVolumeMixer.UI.Contracts.Services;
using SimpleVolumeMixer.UI.Models;
using SimpleVolumeMixer.UI.ViewModels;

namespace SimpleVolumeMixer.Tests.NUnit
{
    public class SettingsViewModelTests
    {
        public SettingsViewModelTests()
        {
        }

        [Test]
        public void TestSettingsViewModel_SetCurrentTheme()
        {
            var mockThemeSelectorService = new Mock<IThemeSelectorService>();
            mockThemeSelectorService.Setup(mock => mock.GetCurrentTheme()).Returns(AppTheme.Light);
            var mockAppConfig = new Mock<AppConfig>();
            var mockSystemService = new Mock<ISystemService>();
            var mockApplicationInfoService = new Mock<IApplicationInfoService>();

            var settingsVm = new SettingsPageViewModel(
                mockAppConfig.Object,
                mockThemeSelectorService.Object,
                mockSystemService.Object,
                mockApplicationInfoService.Object);
            settingsVm.OnNavigatedTo(null);

            Assert.AreEqual(AppTheme.Light, settingsVm.Theme.Value);
        }

        [Test]
        public void TestSettingsViewModel_SetCurrentVersion()
        {
            var mockThemeSelectorService = new Mock<IThemeSelectorService>();
            var mockAppConfig = new Mock<AppConfig>();
            var mockSystemService = new Mock<ISystemService>();
            var mockApplicationInfoService = new Mock<IApplicationInfoService>();
            var testVersion = "1.2.3.4";
            mockApplicationInfoService.Setup(mock => mock.GetAssemblyProductVersion()).Returns(testVersion);

            var settingsVm = new SettingsPageViewModel(
                mockAppConfig.Object,
                mockThemeSelectorService.Object,
                mockSystemService.Object,
                mockApplicationInfoService.Object);
            settingsVm.OnNavigatedTo(null);

            Assert.AreEqual($"SimpleVolumeMixer - {testVersion}", settingsVm.VersionDescription.Value);
        }

        [Test]
        public void TestSettingsViewModel_SetThemeCommand()
        {
            var mockThemeSelectorService = new Mock<IThemeSelectorService>();
            var mockAppConfig = new Mock<AppConfig>();
            var mockSystemService = new Mock<ISystemService>();
            var mockApplicationInfoService = new Mock<IApplicationInfoService>();

            var settingsVm = new SettingsPageViewModel(
                mockAppConfig.Object,
                mockThemeSelectorService.Object,
                mockSystemService.Object,
                mockApplicationInfoService.Object);
            settingsVm.SetThemeCommand.Execute(AppTheme.Light.ToString());

            mockThemeSelectorService.Verify(mock => mock.SetTheme(AppTheme.Light));
        }
    }
}