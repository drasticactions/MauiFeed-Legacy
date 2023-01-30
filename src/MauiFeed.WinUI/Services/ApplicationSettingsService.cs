// <copyright file="ApplicationSettingsService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

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
        private DatabaseContext databaseContext;
        private AppSettings appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSettingsService"/> class.
        /// </summary>
        /// <param name="context">Database Context.</param>
        public ApplicationSettingsService(DatabaseContext context)
        {
            this.databaseContext = context;
            var appSettings = this.databaseContext.AppSettings!.FirstOrDefault();
            if (appSettings is null)
            {
                appSettings = new AppSettings();
                this.databaseContext.AppSettings!.Add(appSettings);
                this.databaseContext.SaveChanges();
            }

            this.appSettings = appSettings;
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

        private void UpdateAppSettings()
        {
            this.databaseContext.AppSettings!.Update(this.appSettings);
            this.databaseContext.SaveChanges();
        }
    }
}
