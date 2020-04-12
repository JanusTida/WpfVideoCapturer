using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CDFCConverters.Converters.DeviceSelectorPage {
    public class LevelToIdentConverter : IValueConverter {
            public object Convert(object value, Type type, object parameter,CultureInfo culture) {
                return new Thickness(System.Convert.ToInt32(value) * c_IndentSize, 0, 0, 0);
            }

            public object ConvertBack(object o, Type type, object parameter,
                                      CultureInfo culture) {
                throw new NotSupportedException();
            }

            private const double c_IndentSize = 15.0;
        }
}
