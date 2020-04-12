using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class FileLocationToUnitStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ulong fileLocationNum = System.Convert.ToUInt64(value);
            return fileLocationNum / 1073741824+"GB";
        }
        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
