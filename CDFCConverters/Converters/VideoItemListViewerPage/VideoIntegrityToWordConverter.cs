using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCConverters.Converters.VideoItemListViewerPage {
    public class VideoIntegrityToWordConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            VideoIntegrity integrity;
            if(Enum.TryParse<VideoIntegrity>(value.ToString(),out integrity)){
                switch (integrity) {
                    case VideoIntegrity.Deleted:
                        return FindResourceString("Deleted");
                    case VideoIntegrity.Covered:
                        return FindResourceString("Covered");
                    case VideoIntegrity.Whole:
                    default:
                        return FindResourceString("Normal");
                }
            }
            else {
                EventLogger.Logger.WriteLine("VideoIntegrityToWordConverter错误:参数转换错误!");
                return FindResourceString("Deleted");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
