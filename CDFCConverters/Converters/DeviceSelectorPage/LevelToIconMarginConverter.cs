using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CDFCConverters.Converters.DeviceSelectorPage {
    public class LevelToIconMarginConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int val = 0;
            Int32.TryParse(value.ToString(), out val);
            return new Thickness(val * 16 + 17, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
