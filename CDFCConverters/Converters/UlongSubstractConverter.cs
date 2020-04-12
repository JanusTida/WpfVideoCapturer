using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class UlongSubstractConverter : IMultiValueConverter {
        private static UlongSubstractConverter staticInstance;
        public static UlongSubstractConverter StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new UlongSubstractConverter());
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            ulong sub1 = 0, sub2 = 0;
            if(values.Length == 2) {
                ulong.TryParse(values[0].ToString(), out sub1);
                ulong.TryParse(values[1].ToString(), out sub2);
                return sub1 - sub2;
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
