using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class StringContainerToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString().Contains(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((bool)value) {
                return parameter.ToString();
            }
            else {
                return "None";
            }
        }
    }
}
