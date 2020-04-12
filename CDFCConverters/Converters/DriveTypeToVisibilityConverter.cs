using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class DriveTypeToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            DriveType type = (DriveType)value;
            if(type == DriveType.Disk) {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
