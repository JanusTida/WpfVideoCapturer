using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class SaveStateToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            byte curEnum = System.Convert.ToByte(value);
            byte para = System.Convert.ToByte(parameter);
            return curEnum == para;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var isChecked = System.Convert.ToBoolean(value);
            if (!isChecked) {
                return null;
            }

            var choice = (SaveState)System.Convert.ToByte(parameter);
            return choice;
        }
    }
}
