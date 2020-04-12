using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters.HomePage {
    public class MouseOverToOpacityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool val;
            if(Boolean.TryParse(value.ToString(),out val) && val){
                if (parameter != null) {
                    try {
                        double opacity = 1.0;
                        if(double.TryParse(parameter.ToString(),out opacity)) {
                            return opacity;
                        }
                        else {
                            return 1.0;
                        }
                    }
                    catch (Exception ex) {
                        EventLogger.Logger.WriteLine("MouseOverToOpacityConverter ->Converter出错:" + ex.Message);
                        return 1.0;
                    }
                }
                else {
                    return 1.0;
                }
            }
            else {
                return 1.0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
