using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CDFCConverters.Converters.DeviceSelectorPage {
    public class StringEqualToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return parameter == null || parameter.ToString() != value.ToString() ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
