// <copyright file="StringNullVisibilityInverter.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MauiFeed.WinUI.Tools
{
    /// <summary>
    /// Null to Visibility Inverter.
    /// </summary>
    public class StringNullVisibilityInverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
