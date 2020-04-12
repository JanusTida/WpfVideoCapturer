using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class BoolToAngleConverteter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((bool)value) {
                return (double)90;
            }
            else {
                return (double)0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
