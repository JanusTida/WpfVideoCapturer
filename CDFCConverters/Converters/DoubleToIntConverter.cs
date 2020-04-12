using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class DoubleToIntConverter : IValueConverter {
        private double max, min;
        public static double Max { get; set; } = 100.0;
        public static double Min { get; set; } = 0.0;
        public DoubleToIntConverter(double max, double min) {
            this.max = max;
            this.min = min;
        }
        public DoubleToIntConverter() {
            this.max = Max;
            this.min = Min;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int resInt;
            try {
                resInt = System.Convert.ToInt32(value);
                return resInt;
            }
            catch {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            double resDouble;
            try {
                resDouble = System.Convert.ToDouble(value);
                if(resDouble > 255) {
                    return 255.0;
                }
                else if(resDouble < 0) {
                    return 0.0;
                }
                else {
                    return resDouble;
                }
            }
            catch {
                return 0.0;
            }
        }
    }
}
