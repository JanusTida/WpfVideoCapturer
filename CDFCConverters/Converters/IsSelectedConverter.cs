using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class IsSelectedConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return System.Convert.ToByte(value) == System.Convert.ToByte(parameter) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value as bool? == true ? System.Convert.ToByte(parameter) : 0;
        }
    }
}
