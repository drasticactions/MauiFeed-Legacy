// <copyright file="ThemeSelectorService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Management.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace MauiFeed.WinUI.Services
{
    public class ThemeSelectorService
    {
        private UISettings uiSettings = new UISettings();
        private const string SettingsKey = "AppBackgroundRequestedTheme";
        private ApplicationDataContainer localSettings;
        private Window window;
        public ElementTheme Theme { get; set; } = ElementTheme.Default;

        public bool IsDark => Theme == ElementTheme.Dark || (Theme == ElementTheme.Default && ThemeSelectorService.IsDarkDefault);

        public static bool IsDarkDefault
        {
            get
            {
                return new Windows.UI.ViewManagement.UISettings().GetColorValue(Windows.UI.ViewManagement.UIColorType.Background) == Color.FromArgb(255, 0, 0, 0);
            }
        }

        public ThemeSelectorService(Window window)
        {
            this.window = window;
            this.localSettings = ApplicationDataManager.CreateForPackageFamily(Package.Current.Id.FamilyName).LocalSettings;
            Theme = LoadThemeFromSettings();
            this.uiSettings.ColorValuesChanged += (sender, args) =>
            {
                if (this.Theme == ElementTheme.Default)
                {
                    this.ThemeChanged?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        public event EventHandler<EventArgs>? ThemeChanged;

        public void SetTheme(ElementTheme theme)
        {
            Theme = theme;

            SetRequestedTheme();
            SaveThemeInSettings(Theme);
        }

        public void SetRequestedTheme()
        {
            if (this.window.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = Theme;
            }

            this.ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private ElementTheme LoadThemeFromSettings()
        {
            ElementTheme cacheTheme = ElementTheme.Default;
            string? themeName = this.localSettings.Values[SettingsKey] as string;

            if (!string.IsNullOrEmpty(themeName))
            {
                Enum.TryParse(themeName, out cacheTheme);
            }

            return cacheTheme;
        }

        private void SaveThemeInSettings(ElementTheme theme)
        {
            this.localSettings.Values[SettingsKey] = theme.ToString();
        }
    }
}
