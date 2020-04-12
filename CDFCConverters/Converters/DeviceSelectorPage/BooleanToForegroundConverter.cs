using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CDFCConverters.Converters.DeviceSelectorPage {
    public class BooleanToForegroundConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool val;
            Boolean.TryParse(value.ToString(),out val);
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = val ? Color.FromRgb(0xff, 0xff, 0xff) : Color.FromRgb(0x1C, 0x99, 0xfb);
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
