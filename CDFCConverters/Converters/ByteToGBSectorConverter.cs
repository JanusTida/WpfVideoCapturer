using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class ByteToGBSectorConverter : IValueConverter {
        private DeviceTypeEnum deviceType {
            get {
                return CDFCSetting.ScanSetting.DeviceType;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ulong size = System.Convert.ToUInt64(value);
            return (size / 1024 / 1024 / 1024).ToString() + "GB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
