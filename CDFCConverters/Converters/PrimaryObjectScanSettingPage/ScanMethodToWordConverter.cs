using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCConverters.Converters.PrimaryObjectScanSettingPage {
    public class ScanMethodToWordConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ScanMethod scanMethod;
            if(Enum.TryParse<ScanMethod>(value.ToString(), out scanMethod)) {
                switch (scanMethod) {
                    case ScanMethod.FileSystem:
                        return FindResourceString("FileSystemScan");
                    case ScanMethod.EntireDisk:
                        return FindResourceString("EntireDiskScan");
                    case ScanMethod.Area:
                        return FindResourceString("AreaScan");
                    case ScanMethod.Left:
                        return FindResourceString("LeftScan");
                    case ScanMethod.MP4:
                        return FindResourceString("Mp4Scan");
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
