using CDFCConverter.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CDFCConverters.Converters.FragmentAnalyzerWindow {
    public class FragmentStatusToColor : IValueConverter {
        public static readonly SolidColorBrush ChosenBrush = new SolidColorBrush { Color = Color.FromRgb(0x17, 0x3c, 0x55) };

        public static readonly Color HeadColor = Colors.Red;
        public static readonly SolidColorBrush HeadBrush = new SolidColorBrush { Color = HeadColor};

        public static readonly Color TileColor = Colors.Black;
        public static readonly SolidColorBrush TileBrush = new SolidColorBrush { Color = TileColor };
        
        public static readonly SolidColorBrush SelectedBrush = new SolidColorBrush { Color = Color.FromRgb(0x33,0x66,0x00) };
        public static readonly SolidColorBrush HasFileBrush = new SolidColorBrush { Color = Color.FromRgb(0x31, 0x85, 0xbf)};
        public static readonly SolidColorBrush NoneBrush = new SolidColorBrush { Color = Color.FromRgb(0xce, 0xce, 0xce) };

        public static readonly SolidColorBrush TickedBrush = new SolidColorBrush { Color = Colors.Yellow };
        //public static readonly SolidColorBrush UnRealBrush = new SolidColorBrush { Color = Colors.Transparent };

        public static readonly LinearGradientBrush HeadAndTileBrush = new LinearGradientBrush {
            GradientStops = new GradientStopCollection {
                new GradientStop(HeadColor,0),
                new GradientStop(TileColor,1)
            }
        };

        private static FragStatusToWord staticInstance;
        public static FragStatusToWord StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new FragStatusToWord());
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            CellStatement cellStatement = (CellStatement)value;
            //if((cellStatement & CellStatement.UnReal) != 0){
            //    return UnRealBrush;
            //}
            //else 
            if ((cellStatement & CellStatement.Ticked) != 0) {
                return TickedBrush;
            }
            //若为头或尾节点;
            else if ((cellStatement & CellStatement.Tile) != 0
                || (cellStatement & CellStatement.Head) != 0) {
                if ((cellStatement & CellStatement.Tile) != 0
                && (cellStatement & CellStatement.Head) != 0) {
                    return HeadBrush;
                }
                else if ((cellStatement & CellStatement.Tile) != 0) {
                    return TileBrush;
                }
                else {
                    return HeadBrush;
                }
            }
            else if ((cellStatement & CellStatement.Selected) != 0) {
                return SelectedBrush;
            }

            //若被文件选中;
            else if((cellStatement & CellStatement.Chosen) != 0) {
                return ChosenBrush;
            }
            
            else if((cellStatement & CellStatement.HasFile) != 0) {
                return HasFileBrush;
            }
            else {
                return NoneBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
