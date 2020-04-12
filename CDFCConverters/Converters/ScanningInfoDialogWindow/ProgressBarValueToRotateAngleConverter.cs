using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters.ScanningInfoDialogWindow {
    public class ProgressBarValueToRotateAngleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int val;
            if(int.TryParse(value.ToString(),out val)) {
                return val * 3.6 -90;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
