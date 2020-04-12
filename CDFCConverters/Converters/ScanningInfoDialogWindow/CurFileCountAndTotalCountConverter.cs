using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters.ScanningInfoDialogWindow {
    /// <summary>
    /// 文件总数与当前文件数的比例显示;
    /// </summary>
    public class CurFileCountAndTotalCountConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            int curCount, total;
            if(values.Length == 2) {
                if (int.TryParse(values[0].ToString(),out curCount)&&int.TryParse(values[1].ToString(),out total)){
                    return string.Format("{0}/{1}", curCount, total);
                }
                else {
                    EventLogger.Logger.WriteLine("CurFileCountAndTotalCountConverter转换出错:值转换错误");
                }
            }

            EventLogger.Logger.WriteLine("CurFileCountAndTotalCountConverter转换出错:集合长度未能匹配");
            return "0/0";
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
