// <copyright file="UIImageExtensions.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

namespace MauiFeed.MacCatalyst.Tools
{
    /// <summary>
    /// UIImage Extensions.
    /// </summary>
    public static class UIImageExtensions
    {
        /// <summary>
        /// Convert a UIImage to one with rounded corners.
        /// </summary>
        /// <param name="image">Image.</param>
        /// <param name="radius">Radius.</param>
        /// <returns>Image with rounded corners.</returns>
        public static UIImage WithRoundedCorners(this UIImage image, nfloat? radius = null)
        {
            var maxRadius = Math.Min(image.Size.Width, image.Size.Height) / 2;
            var cornerRadius = radius.HasValue && radius.Value > 0 && radius.Value <= maxRadius
                ? radius.Value
                : maxRadius;

            UIGraphics.BeginImageContextWithOptions(image.Size, false, image.CurrentScale);
            CGRect rect = new CGRect(CGPoint.Empty, image.Size);
            UIBezierPath path = UIBezierPath.FromRoundedRect(rect, (nfloat)cornerRadius);
            path.AddClip();
            image.Draw(rect);
            UIImage roundedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return roundedImage;
        }
    }
}