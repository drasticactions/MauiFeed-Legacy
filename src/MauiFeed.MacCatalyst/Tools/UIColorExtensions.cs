// <copyright file="UIColorExtensions.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

namespace MauiFeed.MacCatalyst.Tools
{
    /// <summary>
    /// UIColor Extensions.
    /// </summary>
    public static class UIColorExtensions
    {
        /// <summary>
        /// Gets the System Tint.
        /// </summary>
        /// <returns><see cref="UIColor"/>.</returns>
        public static UIColor GetSystemTint()
        {
#if TVOS
            return UIColor.Clear;
#else
            var color = UIConfigurationColorTransformer.PreferredTint;
            return color(UIColor.Clear);
#endif
        }
    }
}