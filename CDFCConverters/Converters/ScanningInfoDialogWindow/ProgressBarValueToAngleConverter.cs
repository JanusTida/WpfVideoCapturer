using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters.ScanningInfoDialogWindow {
    /// <summary>
    /// 进度条进度转化为圆弧角度的转换器;
    /// </summary>
    public class ProgressBarValueToAngleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int val;
            if(int.TryParse(value.ToString(),out val)) {
                return val * 3.6;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
