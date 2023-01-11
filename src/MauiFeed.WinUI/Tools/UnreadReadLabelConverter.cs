// <copyright file="UnreadReadLabelConverter.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;
using Microsoft.UI.Xaml.Data;

namespace MauiFeed.WinUI
{
    public class UnreadReadLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Translations.Common.MarkAsUnreadLabel : Translations.Common.MarkAsReadLabel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AllUnreadReadLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Translations.Common.MarkAllAsUnrealLabel : Translations.Common.MarkAllAsReadLabel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
