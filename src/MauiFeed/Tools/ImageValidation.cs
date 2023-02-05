// <copyright file="ImageValidation.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Text;
using MauiFeed.Models;

namespace MauiFeed.Tools
{
    /// <summary>
    /// Image Validation.
    /// </summary>
    public static class ImageValidation
    {
        private static Dictionary<string, byte[]> validBytes = new Dictionary<string, byte[]>()
        {
        { ".bmp", new byte[] { 66, 77 } },
        { ".gif", new byte[] { 71, 73, 70, 56 } },
        { ".ico", new byte[] { 0, 0, 1, 0 } },
        { ".jpg", new byte[] { 255, 216, 255 } },
        { ".png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 } },
        { ".tiff", new byte[] { 73, 73, 42, 0 } },
        };

        /// <summary>
        /// Checks a byte array to see if it's a valid image.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        /// <returns>Is valid image.</returns>
        public static bool IsValidImage(byte[] bytes)
        {
            return validBytes.Any(x => x.Value.SequenceEqual(bytes.Take(x.Value.Length)));
        }

        /// <summary>
        /// Validates if the image on an FeedListItem is okay.
        /// </summary>
        /// <param name="item">FeedListItem.</param>
        /// <returns>Bool.</returns>
        public static bool HasValidImage(this FeedListItem item)
        {
            return item.ImageCache != null && item.ImageCache.Length > 0 && IsValidImage(item.ImageCache);
        }
    }
}
