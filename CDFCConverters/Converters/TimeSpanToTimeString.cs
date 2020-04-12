using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class TimeSpanToTimeString : IValueConverter {
        private static TimeSpanToTimeString staticInstance;
        public static TimeSpanToTimeString StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new TimeSpanToTimeString());
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            TimeSpan ts = (TimeSpan)value;
            return (ts.Days == 0?string.Empty:(ts.Days+"-")) + string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
