// <copyright file="UIColorExtensions.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

namespace MauiFeed.Apple
{
    public static class UIColorExtensions
    {
        public static UIColor GetSystemTint()
        {
#if TVOS
            return UIColor.Clear;
#else
            var poop = UIConfigurationColorTransformer.PreferredTint;
            return poop(UIColor.Clear);
#endif
        }
    }
}