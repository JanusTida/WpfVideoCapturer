using System;
using System.Globalization;
using System.Windows.Data;
using CDFCConverter.Enums;

namespace CDFCConverters.Converters {
    public class FragStatusToWord : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var cellStatement = (CellStatement)value;
            if((cellStatement & CellStatement.Tile) != 0) {
                return "文件尾";
            }
            else if((cellStatement & CellStatement.Head) != 0) {
                return "文件头";
            }
            else {
                return "文件碎片";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
