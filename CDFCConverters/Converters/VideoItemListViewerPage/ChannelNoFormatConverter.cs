using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CDFCConverters.Converters.VideoItemListViewerPage {
    public class ChannelNoFormatConverter : IValueConverter {
        private static ChannelNoFormatConverter staticInstance;
        public static ChannelNoFormatConverter StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new ChannelNoFormatConverter());
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int valNum = 0;
            var valString = value.ToString();
            if (int.TryParse(valString,out valNum)){
                if (valNum <= 200) {
                    return valNum.ToString();
                }
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
