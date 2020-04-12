using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class ChannelNoToChdConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var channelNum = parameter.ToString();

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {

            throw new NotImplementedException();
        }
    }
}
