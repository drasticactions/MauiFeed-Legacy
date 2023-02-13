// <copyright file="CatalystPlatformService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

namespace MauiFeed.MacCatalyst.Services
{
    /// <summary>
    /// Catalyst Platform Services.
    /// </summary>
    public class CatalystPlatformService
    {
        private UIWindow? window;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalystPlatformService"/> class.
        /// </summary>
        public CatalystPlatformService()
        {
        }

        /// <summary>
        /// Set the main window.
        /// </summary>
        /// <param name="window">Window.</param>
        public void SetMainWindow(UIWindow window)
            => this.window = window;

        /// <summary>
        /// Sets the title on the app window.
        /// </summary>
        /// <param name="title">Title.</param>
        public void SetTitle(string? title = default)
        {
            if (this.window?.WindowScene is not UIWindowScene scene)
            {
                return;
            }

            scene.Title = title ?? Translations.Common.AppTitle;
        }
    }
}