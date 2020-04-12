using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class DateTimeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var dateTime = (DateTime?)value;
            if(dateTime != null && dateTime.HasValue && dateTime != DateTime.MinValue) {
                var val = dateTime.Value;
                return string.Format("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}",
                    val.Year, val.Month, val.Day, val.Hour, val.Minute, val.Second);
            }
            else {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
