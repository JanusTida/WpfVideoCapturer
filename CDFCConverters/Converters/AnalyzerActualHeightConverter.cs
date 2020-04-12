using System;
using System.Globalization;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    public class AnalyzerActualHeightConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double outterHeight = System.Convert.ToDouble(value);
            return outterHeight - 40.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
