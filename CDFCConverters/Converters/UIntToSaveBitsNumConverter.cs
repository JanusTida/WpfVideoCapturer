using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class UIntToSaveBitsNumConverter : IValueConverter {
        public static DeviceTypeEnum DeviceTypeEnum { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            byte bits = System.Convert.ToByte(parameter);
            uint num = System.Convert.ToUInt32(value);
//            num = DeviceTypeEnum == DeviceTypeEnum.DaHua ? num  : num;
            return string.Format("{0:D"+bits+"}", num);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
