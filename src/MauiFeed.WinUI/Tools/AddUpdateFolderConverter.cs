using MauiFeed.Models;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiFeed.WinUI
{
    public class AddUpdateFolderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not FeedFolder folder)
            {
                return string.Empty;
            }

            return folder.Id > 0 ? Translations.Common.UpdateFolderLabel : Translations.Common.AddFolderLabel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
