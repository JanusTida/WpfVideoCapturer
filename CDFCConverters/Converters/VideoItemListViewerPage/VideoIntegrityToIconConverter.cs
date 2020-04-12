using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters.VideoItemListViewerPage {
    public class VideoIntegrityToIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            VideoIntegrity integrity;
            if (Enum.TryParse<VideoIntegrity>(value.ToString(), out integrity)) {
                switch (integrity) {
                    case VideoIntegrity.Deleted:
                        return "/CDFCVideoExactor;component/Images/VideoItemListViewerPage/VideoStateDeleted.png";
                    case VideoIntegrity.Covered:
                        return "/CDFCVideoExactor;component/Images/VideoItemListViewerPage/VideoStateCovered.png";
                    case VideoIntegrity.Whole:
                    default:
                        return "/CDFCVideoExactor;component/Images/VideoItemListViewerPage/VideoStateCompleted.png";
                }
            }
            else {
                EventLogger.Logger.WriteLine("VideoIntegrityToWordConverter错误:参数转换错误!");
                return "/CDFCVideoExactor;component/Images/VideoItemListViewerPage/VideoStateCompleted.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
