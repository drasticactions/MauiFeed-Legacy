// <copyright file="EnumToBooleanConverter.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

#nullable disable

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Data;

namespace MauiFeed.WinUI
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public Type EnumType { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
            {
                if (!Enum.IsDefined(EnumType, value))
                {
                    throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum".GetLocalized());
                }

                var enumValue = Enum.Parse(EnumType, enumString);

                return enumValue.Equals(value);
            }

            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName".GetLocalized());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
            {
                return Enum.Parse(EnumType, enumString);
            }

            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName".GetLocalized());
        }
    }
}
