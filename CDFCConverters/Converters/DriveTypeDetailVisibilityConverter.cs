using EventLogger;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    class DriveTypeDetailVisibilityConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if(values[0] == null || values[1] == null) {
                return Visibility.Collapsed;
            }
            else {
                try { 
                    var driveFormat = values[0].ToString();
                    var showDetail = (bool)values[1];
                    if(showDetail && driveFormat == parameter.ToString()) {
                        return Visibility.Visible;
                    }
                    else {
                        return Visibility.Collapsed;
                    }
                }
                catch(Exception ex) {
                    Logger.WriteLine(ex.Message);
                    return Visibility.Collapsed;
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
