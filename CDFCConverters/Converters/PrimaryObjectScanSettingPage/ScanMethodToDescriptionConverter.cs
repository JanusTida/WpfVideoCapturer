using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCConverters.Converters.PrimaryObjectScanSettingPage {
    public class ScanMethodToDescriptionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ScanMethod scanMethod;
            if(Enum.TryParse(value.ToString(),out scanMethod)) {
                switch (scanMethod) {
                    case ScanMethod.EntireDisk:
                        return FindResourceString("EntireDisc");
                    case ScanMethod.FileSystem:
                        return FindResourceString("FileSytemDisc");
                    case ScanMethod.Left:
                        return FindResourceString("LeftDisc");
                    case ScanMethod.MP4:
                        return FindResourceString("Mp4Disc");
                    case ScanMethod.Area:
                        return FindResourceString("AreaDisc");
                    default:
                        break;
                }
            }
            return "。";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
