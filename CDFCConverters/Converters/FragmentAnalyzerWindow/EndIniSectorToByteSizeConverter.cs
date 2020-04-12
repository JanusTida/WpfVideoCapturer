using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CDFCConverters.Converters.FragmentAnalyzerWindow {
    public class EndIniSectorToByteSizeConverter : IMultiValueConverter {
        private static EndIniSectorToByteSizeConverter staticInstance;
        public static EndIniSectorToByteSizeConverter StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new EndIniSectorToByteSizeConverter());
            }
        }
        public static int SectorSize {
            get {
                return CDFCSetting.ScanSetting.SectorSize;
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if(values.Length != 2) {
                return string.Empty;
            }
            ulong sub1 = 0, sub2 = 0;
            ulong.TryParse(values[0].ToString(), out sub1);
            ulong.TryParse(values[1].ToString(), out sub2);
            ulong val = 0;
            val = (sub1 - sub2) * (ulong)SectorSize;

            if (val > (ulong)1024 * 1024 * 1024 * 1024) {
                return string.Format("{0:F1} T", val / ((double)1024 * 1024 * 1024 * 1024));
            }
            else if (val > (ulong)1024 * 1024 * 1024) {
                return string.Format("{0:F1} G", val / ((double)1024 * 1024 * 1024));
            }
            else if (val > (ulong)1024 * 1024) {
                return string.Format("{0:F1} M", val / ((double)1024 * 1024));
            }
            else if (val > 1024) {
                return string.Format("{0:F1} K", val / (double)1024);
            }
            else {
                return string.Format("{0:F1} B", (double)val);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
