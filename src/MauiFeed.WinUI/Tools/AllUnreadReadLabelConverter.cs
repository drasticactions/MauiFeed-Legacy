// <copyright file="AllUnreadReadLabelConverter.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Data;

namespace MauiFeed.WinUI.Tools
{
    /// <summary>
    /// All Unread Read Label Converter.
    /// </summary>
    public class AllUnreadReadLabelConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Translations.Common.MarkAllAsUnrealLabel : Translations.Common.MarkAllAsReadLabel;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
