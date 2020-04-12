using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CDFCConverters.Converters.FragmentAnalyzerWindow {
    public class SectorSizeToByteSizeConverter : IValueConverter {
        private static SectorSizeToByteSizeConverter staticInstance;
        public static SectorSizeToByteSizeConverter StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new  SectorSizeToByteSizeConverter());
            }
        }
        public static int SectorSize {
            get {
                return CDFCSetting.ScanSetting.SectorSize;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ulong val = 0;
            ulong.TryParse(value.ToString(), out val);
            val *= (ulong)SectorSize;
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
                return string.Format("{0:F1} B",(double) val);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
