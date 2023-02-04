// <copyright file="ThemeSelectorService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;
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
        private UISettings uiSettings = new UISettings();
        private WindowService windowService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeSelectorService"/> class.
        /// </summary>
        /// <param name="windowService"><see cref="WindowService"/>.</param>
        /// <param name="applicationSettingsService">App Settings Service.</param>
        public ThemeSelectorService(WindowService windowService)
        {
            this.windowService = windowService;
            this.windowService.WindowAdded += this.WindowServiceWindowAdded;
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
        /// Sets the requested theme on a window.
        /// </summary>
        /// <param name="window">The Window.</param>
        public void SetRequestedThemeOnWindow(Window window)
        {
            if (window.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = this.Theme;
            }
        }

        /// <summary>
        /// Sets the current application theme for all windows.
        /// </summary>
        /// <param name="theme">The theme to change to.</param>
        public void SetTheme(ElementTheme theme)
        {
            this.Theme = theme;

            this.SetRequestedTheme();
        }

        /// <summary>
        /// Set the requested theme.
        /// </summary>
        public void SetRequestedTheme()
        {
            foreach (var window in this.windowService.ApplicationWindows)
            {
                this.SetRequestedThemeOnWindow((Window)window);
            }

            this.ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private ElementTheme GetElementTheme(AppTheme theme)
        {
            switch (theme)
            {
                case AppTheme.Default:
                    return ElementTheme.Default;
                case AppTheme.Light:
                    return ElementTheme.Light;
                case AppTheme.Dark:
                    return ElementTheme.Dark;
                default:
                    return ElementTheme.Default;
            }
        }

        private AppTheme GetAppTheme(ElementTheme theme)
        {
            switch (theme)
            {
                case ElementTheme.Default:
                    return AppTheme.Default;
                case ElementTheme.Light:
                    return AppTheme.Light;
                case ElementTheme.Dark:
                    return AppTheme.Dark;
                default:
                    return AppTheme.Default;
            }
        }

        private void WindowServiceWindowAdded(object? sender, Events.WindowAddedEventArgs e)
        {
            this.SetRequestedThemeOnWindow(e.Window);
        }
    }
}
