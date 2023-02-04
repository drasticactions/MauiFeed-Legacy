// <copyright file="AddUpdateFolderConverter.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Models;
using Microsoft.UI.Xaml.Data;

namespace MauiFeed.WinUI.Tools
{
    /// <summary>
    /// Add or update folder converter.
    /// </summary>
    public class AddUpdateFolderConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not FeedFolder folder)
            {
                return string.Empty;
            }

            return folder.Id > 0 ? Translations.Common.UpdateFolderLabel : Translations.Common.AddFolderLabel;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
