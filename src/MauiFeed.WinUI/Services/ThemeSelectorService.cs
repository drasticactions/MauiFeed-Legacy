// <copyright file="ThemeSelectorService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Management.Core;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace MauiFeed.WinUI.Services
{
    /// <summary>
    /// Theme Selector Service.
    /// </summary>
    public class ThemeSelectorService
    {
        private const string SettingsKey = "AppBackgroundRequestedTheme";
        private UISettings uiSettings = new UISettings();
        private ApplicationDataContainer localSettings;
        private WindowService windowService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeSelectorService"/> class.
        /// </summary>
        /// <param name="windowService"><see cref="WindowService"/>.</param>
        public ThemeSelectorService(WindowService windowService)
        {
            this.windowService = windowService;
            this.localSettings = ApplicationDataManager.CreateForPackageFamily(Package.Current.Id.FamilyName).LocalSettings;
            this.Theme = this.LoadThemeFromSettings();
            this.uiSettings.ColorValuesChanged += (sender, args) =>
            {
                if (this.Theme == ElementTheme.Default)
                {
                    this.ThemeChanged?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        /// <summary>
        /// Fired when the theme changes.
        /// </summary>
        public event EventHandler<EventArgs>? ThemeChanged;

        /// <summary>
        /// Gets a value indicating whether the dark theme is the default on the system.
        /// </summary>
        public static bool IsDarkDefault
        {
            get
            {
                return new UISettings().GetColorValue(UIColorType.Background) == Windows.UI.Color.FromArgb(255, 0, 0, 0);
            }
        }

        /// <summary>
        /// Gets the current application theme.
        /// </summary>
        public ElementTheme Theme { get; private set; } = ElementTheme.Default;

        /// <summary>
        /// Gets a value indicating whether the current theme is dark.
        /// </summary>
        public bool IsDark => this.Theme == ElementTheme.Dark || (this.Theme == ElementTheme.Default && ThemeSelectorService.IsDarkDefault);

        /// <summary>
        /// Sets the current application theme for all windows.
        /// </summary>
        /// <param name="theme">The theme to change to.</param>
        public void SetTheme(ElementTheme theme)
        {
            this.Theme = theme;

            this.SetRequestedTheme();
            this.SaveThemeInSettings(this.Theme);
        }

        /// <summary>
        /// Set the requested theme.
        /// </summary>
        public void SetRequestedTheme()
        {
            foreach (var window in this.windowService.ApplicationWindows)
            {
                if (window.Content is FrameworkElement frameworkElement)
                {
                    frameworkElement.RequestedTheme = this.Theme;
                }
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
