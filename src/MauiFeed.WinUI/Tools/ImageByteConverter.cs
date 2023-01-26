// <copyright file="ImageByteConverter.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace MauiFeed.WinUI.Tools
{
    /// <summary>
    /// Image Byte Converter.
    /// </summary>
    public class ImageByteConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not byte[] imageArray)
            {
                return null;
            }

            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(imageArray);
                    writer.StoreAsync().GetResults();
                }

                BitmapImage image = new BitmapImage();
                image.SetSource(ms);
                return image;
            }
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
