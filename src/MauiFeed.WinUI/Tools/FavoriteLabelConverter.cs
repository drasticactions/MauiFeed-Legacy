// <copyright file="FavoriteLabelConverter.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;
using Microsoft.UI.Xaml.Data;

namespace MauiFeed.WinUI
{
    public class FavoriteLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Translations.Common.RemoveStarLabel : Translations.Common.MarkAsStarLabel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
