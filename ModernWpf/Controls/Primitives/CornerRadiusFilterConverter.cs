﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    public class CornerRadiusFilterConverter : DependencyObject, IValueConverter
    {
        public CornerRadiusFilterKind Filter
        {
            get => (CornerRadiusFilterKind)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                nameof(Filter),
                typeof(CornerRadiusFilterKind),
                typeof(CornerRadiusFilterConverter));

        public CornerRadius Convert(CornerRadius radius, CornerRadiusFilterKind filterKind)
        {
            CornerRadius result = radius;

            switch (filterKind)
            {
                case CornerRadiusFilterKind.Top:
                    result.BottomLeft = 0;
                    result.BottomRight = 0;
                    break;
                case CornerRadiusFilterKind.Right:
                    result.TopLeft = 0;
                    result.BottomLeft = 0;
                    break;
                case CornerRadiusFilterKind.Bottom:
                    result.TopLeft = 0;
                    result.TopRight = 0;
                    break;
                case CornerRadiusFilterKind.Left:
                    result.TopRight = 0;
                    result.BottomRight = 0;
                    break;
            }

            return result;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cornerRadius = (CornerRadius)value;
            var filterType = Filter;
            if (filterType == CornerRadiusFilterKind.TopLeftValue ||
                filterType == CornerRadiusFilterKind.BottomRightValue)
            {
                return GetDoubleValue(cornerRadius, filterType);
            }

            return Convert(cornerRadius, filterType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private double GetDoubleValue(CornerRadius radius, CornerRadiusFilterKind filterKind)
        {
            switch (filterKind)
            {
                case CornerRadiusFilterKind.TopLeftValue:
                    return radius.TopLeft;
                case CornerRadiusFilterKind.BottomRightValue:
                    return radius.BottomRight;
            }
            return 0;
        }
    }

    public enum CornerRadiusFilterKind
    {
        None,
        Top,
        Right,
        Bottom,
        Left,
        TopLeftValue,
        BottomRightValue
    }
}
