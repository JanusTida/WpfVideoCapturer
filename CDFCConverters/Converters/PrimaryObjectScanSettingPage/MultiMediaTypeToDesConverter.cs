using CDFCConverters.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CDFCConverters.Converters.PrimaryObjectScanSettingPage {
    public class MultiMediaTypeToDesConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            MultiMediaType mType;
            if (Enum.TryParse<MultiMediaType>(value.ToString(), out mType)) {
                switch (mType) {
                    case MultiMediaType.Camera:
                        return "我是多媒体录像机的介绍";
                    case MultiMediaType.DrivingRecorder:
                        return "我是行车记录仪的介绍";
                    case MultiMediaType.ForensicRecorder:
                        return "我是执法记录仪的介绍";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
