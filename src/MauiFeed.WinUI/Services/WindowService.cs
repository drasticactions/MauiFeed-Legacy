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

        /// <summary>
        /// Add Window.
        /// </summary>
        /// <typeparam name="T">Window Type.</typeparam>
        /// <param name="args">Window Arguments.</param>
        /// <returns>New Window.</returns>
        /// <exception cref="ArgumentException">Thrown if parameter is not window.</exception>
        public T AddWindow<T>(object?[]? args = default)
        {
            var instance = (T)Activator.CreateInstance(typeof(T), args)!;
            if (instance is not Window win)
            {
                throw new ArgumentException("Parameter must be window");
            }

            this.ApplicationWindows.Add(win);
            win.Closed += this.WinClosed;

            return instance;
        }

        private void WinClosed(object sender, WindowEventArgs args)
        {
            ((Window)sender).Closed -= this.WinClosed;
            this.ApplicationWindows.Remove((Window)sender);
        }
    }
}
