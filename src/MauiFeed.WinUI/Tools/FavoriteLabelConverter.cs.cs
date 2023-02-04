// <copyright file="FavoriteLabelConverter.cs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Data;

namespace MauiFeed.WinUI.Tools
{
    /// <summary>
    /// Favorite Label Converter.
    /// </summary>
    public class FavoriteLabelConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Translations.Common.RemoveStarLabel : Translations.Common.MarkAsStarLabel;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
