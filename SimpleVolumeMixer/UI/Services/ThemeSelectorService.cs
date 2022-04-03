using System;
using System.Linq;
using System.Windows;
using ControlzEx.Theming;
using MahApps.Metro.Theming;
using MaterialDesignThemes.MahApps;
using MaterialDesignThemes.Wpf;
using SimpleVolumeMixer.UI.Contracts.Services;
using SimpleVolumeMixer.UI.Models;

namespace SimpleVolumeMixer.UI.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string HcDarkTheme = "pack://application:,,,/UI/Styles/Themes/HC.Dark.Blue.xaml";
    private const string HcLightTheme = "pack://application:,,,/UI/Styles/Themes/HC.Light.Blue.xaml";

    public void InitializeTheme()
    {
        // TODO WTS: Mahapps.Metro supports syncronization with high contrast but you have to provide custom high contrast themes
        // We've added basic high contrast dictionaries for Dark and Light themes
        // Please complete these themes following the docs on https://mahapps.com/docs/themes/thememanager#creating-custom-themes
        // ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcDarkTheme),
        //     MahAppsLibraryThemeProvider.DefaultInstance));
        // ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcLightTheme),
        //     MahAppsLibraryThemeProvider.DefaultInstance));

        var theme = GetCurrentTheme();
        SetTheme(theme);
    }

    public void SetTheme(AppTheme theme)
    {
        var mahAppBundledTheme =
            Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is MahAppsBundledTheme) as
                MahAppsBundledTheme;

        if (theme == AppTheme.Default)
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();

            if (mahAppBundledTheme != null) mahAppBundledTheme.BaseTheme = BaseTheme.Inherit;
        }
        else
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast;
            ThemeManager.Current.SyncTheme();
            ThemeManager.Current.ChangeTheme(Application.Current, $"{theme}.Steel", SystemParameters.HighContrast);

            if (mahAppBundledTheme != null)
            {
                mahAppBundledTheme.BaseTheme = theme == AppTheme.Light ? BaseTheme.Light : BaseTheme.Dark;
            }
        }

        Application.Current.Properties["Theme"] = theme.ToString();
    }

    public AppTheme GetCurrentTheme()
    {
        if (Application.Current.Properties.Contains("Theme"))
        {
            var themeName = Application.Current.Properties["Theme"].ToString();
            Enum.TryParse(themeName, out AppTheme theme);
            return theme;
        }

        return AppTheme.Default;
    }
}