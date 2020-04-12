using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters.PrimaryObjectScanSettingPage {
    public class ScanMethodToCheckedConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ScanMethod method;
             if(Enum.TryParse<ScanMethod>(value.ToString(), out method)) {
                ScanMethod curMethod;
                if (Enum.TryParse<ScanMethod>(parameter.ToString(),out curMethod)) {
                    return curMethod == method;
                }
                return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            bool isChecked = (bool)value;
            ScanMethod curMethod;

            if (!isChecked) {
                return null;
            }
            if (Enum.TryParse(parameter.ToString(), out curMethod)) {
                return curMethod;
            }
            return ScanMethod.EntireDisk;
        }
    }
}
