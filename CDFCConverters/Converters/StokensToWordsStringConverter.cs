using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CDFCConverters.Converters {
    class StokensToWordsStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            StringBuilder sb = new StringBuilder();
            string valString = value.ToString();
            int nowIndex = 0;
            int newLineIndex;
            string newLine = Environment.NewLine;
            while ((newLineIndex = valString.IndexOf(newLine,nowIndex)) != -1) {
                string lineString = valString.Substring(nowIndex, newLineIndex - nowIndex);
                string[] stokens = lineString.Split(' ');
                foreach(var p in stokens) {
                    if (!string.IsNullOrWhiteSpace(p)) {
                        if (string.IsNullOrWhiteSpace(p)) {
                            if(p != " ") { 
                                sb.AppendLine();
                            }
                        }else { 
                            char ch = System.Convert.ToChar(int.Parse(p, NumberStyles.HexNumber));
                            if (!char.IsControl(ch)) { 
                                sb.Append(ch.ToString());
                            }
                            else {
                                sb.Append('_');
                            }
                        }
                    }
                }
                nowIndex = newLineIndex + 2;
                
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
