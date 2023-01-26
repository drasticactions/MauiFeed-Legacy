// <copyright file="WindowService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml;

namespace MauiFeed.WinUI.Services
{
    /// <summary>
    /// Window Service.
    /// </summary>
    public class WindowService
    {
        /// <summary>
        /// Gets the current application windows.
        /// </summary>
        public IList<Window> ApplicationWindows { get; } = new List<Window>();
    }
}
