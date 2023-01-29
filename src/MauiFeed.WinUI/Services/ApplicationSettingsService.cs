// <copyright file="ApplicationSettingsService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

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
        private const string AppBackgroundRequestedTheme = "AppBackgroundRequestedTheme";
        private const string LastUpdatedValue = "LastUpdated";
        private ApplicationDataContainer localSettings;
        private ElementTheme? cachedTheme;
        private DateTime? cachedTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSettingsService"/> class.
        /// </summary>
        public ApplicationSettingsService()
        {
            this.localSettings = ApplicationDataManager.CreateForPackageFamily(Package.Current.Id.FamilyName).LocalSettings;
        }

        /// <summary>
        /// Gets or sets the Last Updated Time.
        /// </summary>
        public DateTime? LastUpdated
        {
            get
            {
                if (this.cachedTime is not null)
                {
                    return this.cachedTime;
                }

                DateTime dateTime = DateTime.MinValue;
                var result = DateTime.TryParse(this.localSettings.Values[LastUpdatedValue] as string, out dateTime);
                if (result)
                {
                    this.cachedTime = dateTime;
                }

                return this.cachedTime;
            }

            set
            {
                this.localSettings.Values[LastUpdatedValue] = value.ToString();
                this.cachedTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the application theme.
        /// </summary>
        public ElementTheme ApplicationElementTheme
        {
            get
            {
                if (this.cachedTheme is not null)
                {
                    return (ElementTheme)this.cachedTheme;
                }

                ElementTheme cacheTheme = ElementTheme.Default;
                string? themeName = this.localSettings.Values[AppBackgroundRequestedTheme] as string;

                if (!string.IsNullOrEmpty(themeName))
                {
                    Enum.TryParse(themeName, out cacheTheme);
                }

                this.cachedTheme = cacheTheme;
                return cacheTheme;
            }

            set
            {
                this.localSettings.Values[AppBackgroundRequestedTheme] = value.ToString();
                this.cachedTheme = value;
            }
        }
    }
}
