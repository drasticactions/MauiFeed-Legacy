// <copyright file="WindowAddedEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml;

namespace MauiFeed.WinUI.Events
{
    /// <summary>
    /// Window Added Event Args.
    /// </summary>
    public class WindowAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowAddedEventArgs"/> class.
        /// </summary>
        /// <param name="window">Get the window.</param>
        public WindowAddedEventArgs(Window window)
        {
            this.Window = window;
        }

        /// <summary>
        /// Gets the new window.
        /// </summary>
        public Window Window { get; private set; }
    }
}
