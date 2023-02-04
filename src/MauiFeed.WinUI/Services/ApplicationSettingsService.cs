// <copyright file="ApplicationSettingsService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Globalization;
using MauiFeed.Models;
using MauiFeed.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel;
using Windows.Management.Core;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace MauiFeed.WinUI.Services
{
    /// <summary>
    /// Application Settings Service.
    /// </summary>
    public class ApplicationSettingsService
    {
        private ThemeSelectorService themeSelectorService;
        private DatabaseContext databaseContext;
        private AppSettings appSettings;
        private CultureInfo defaultCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSettingsService"/> class.
        /// </summary>
        /// <param name="context">Database Context.</param>
        /// <param name="themeSelectorService">Theme selector service.</param>
        public ApplicationSettingsService(DatabaseContext context, ThemeSelectorService themeSelectorService)
        {
            this.databaseContext = context;
            this.defaultCulture = Thread.CurrentThread.CurrentUICulture;
            var appSettings = this.databaseContext.AppSettings!.FirstOrDefault();
            if (appSettings is null)
            {
                appSettings = new AppSettings();
                this.databaseContext.AppSettings!.Add(appSettings);
                this.databaseContext.SaveChanges();
            }

            this.appSettings = appSettings;
            this.themeSelectorService = themeSelectorService;
        }

        /// <summary>
        /// Gets or sets the Last Updated Time.
        /// </summary>
        public DateTime? LastUpdated
        {
            get
            {
                return this.appSettings.LastUpdated;
            }

            set
            {
                this.appSettings.LastUpdated = value;
                this.UpdateAppSettings();
            }
        }

        /// <summary>
        /// Gets or sets the application theme.
        /// </summary>
        public LanguageSetting ApplicationLanguageSetting
        {
            get
            {
                return this.appSettings.LanguageSetting;
            }

            set
            {
                this.appSettings.LanguageSetting = value;
                this.UpdateAppSettings();
            }
        }

        /// <summary>
        /// Gets or sets the application theme.
        /// </summary>
        public AppTheme ApplicationElementTheme
        {
            get
            {
                return this.appSettings.AppTheme;
            }

            set
            {
                this.appSettings.AppTheme = value;
                this.UpdateAppSettings();
            }
        }

        /// <summary>
        /// Refresh the app with the given app settings.
        /// </summary>
        public void RefreshApp()
        {
            this.UpdateCulture();
            this.UpdateTheme();
        }

        private void UpdateCulture()
        {
            var culture = this.defaultCulture;
            switch (this.ApplicationLanguageSetting)
            {
                case LanguageSetting.English:
                    culture = new CultureInfo("en-US");
                    break;
                case LanguageSetting.Japanese:
                    culture = new CultureInfo("ja-JP");
                    break;
            }

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void UpdateAppSettings()
        {
            this.databaseContext.AppSettings!.Update(this.appSettings);
            this.databaseContext.SaveChanges();
            this.RefreshApp();
        }

        private void UpdateTheme()
        {
            ElementTheme theme;

            switch (this.ApplicationElementTheme)
            {
                case AppTheme.Default:
                    theme = ElementTheme.Default;
                    break;
                case AppTheme.Light:
                    theme = ElementTheme.Light;
                    break;
                case AppTheme.Dark:
                    theme = ElementTheme.Dark;
                    break;
                default:
                    theme = ElementTheme.Default;
                    break;
            }

            this.themeSelectorService.SetTheme(theme);
        }
    }
}