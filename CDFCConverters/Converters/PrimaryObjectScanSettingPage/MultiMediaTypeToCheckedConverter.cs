using CDFCConverters.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CDFCConverters.Converters.PrimaryObjectScanSettingPage {
    public class MultiMediaTypeToCheckedConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            MultiMediaType mType;
            if (Enum.TryParse<MultiMediaType>(value.ToString(), out mType)) {
                MultiMediaType curType;
                if (Enum.TryParse<MultiMediaType>(parameter.ToString(), out curType)) {
                    return mType == curType;
                }
                return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            bool isChecked = (bool)value;
            MultiMediaType curType;

            if (!isChecked) {
                return null;
            }
            if (Enum.TryParse(parameter.ToString(), out curType)) {
                return curType;
            }
            return MultiMediaType.Camera;
        }
    }
}
