// <copyright file="EmbeddedImageConverter.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MauiFeed.WinUI.Tools
{
    /// <summary>
    /// Embedded Image Converter.
    /// </summary>
    public class EmbeddedImageConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var logo = Utilities.GetResourceFileContent((string)value)!;
            var image = new BitmapImage();
            image.SetSource(logo.AsRandomAccessStream());
            return image;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
