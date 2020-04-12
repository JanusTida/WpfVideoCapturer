using CDFCConverters.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    /// <summary>
    /// bool与是否可见的转换器;
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter {
        private static BoolToVisibilityConverter staticInstance;
        public static BoolToVisibilityConverter StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new BoolToVisibilityConverter());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">调整输出的参数,是否反转，是否保留(hidden,collapsed)</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool val;
            VisibilityAttributes attributes = VisibilityAttributes.Normal;
            Boolean.TryParse(value.ToString(), out val);
            if(parameter != null) {
                Enum.TryParse<VisibilityAttributes>(parameter.ToString(), out attributes);
            }
            if((attributes & VisibilityAttributes.Reverse ) == VisibilityAttributes.Reverse) {
                return val ? (attributes & VisibilityAttributes.Save) == VisibilityAttributes.Save ?
                    Visibility.Hidden : Visibility.Collapsed:
                    Visibility.Visible;
            }
            else {
                return val ? Visibility.Visible :
                    (attributes & VisibilityAttributes.Save) == VisibilityAttributes.Save ?
                    Visibility.Hidden : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
