using CDFCConverter.Enums;
using CDFCVideoExactor.Abstracts;
using System.Collections.Generic;

namespace CDFCVideoExactor.Models {
    //碎片单位;
    public class ObjectSectorCell : NotificationObject {
        //初始扇区地址;
        private ulong iniAddress;
        public ulong IniSector {
            get {
                return iniAddress;
            }
            set {
                iniAddress = value;
                NotifyPropertyChanging(nameof(IniSector));
            }
        }
        //终止扇区地址;
        public ulong EndSector { get; set; }

        //单元的显示宽度;
        private double width = 10;
        public double Width {
            get {
                return width;
            }
            set {
                width = value;
                NotifyPropertyChanging(nameof(Width));
            }
        }

        //单元的显示高度;
        private double height = 10;
        public double Height {
            get {
                return height;
            }
            set {
                height = value;
                NotifyPropertyChanging(nameof(Height));
            }
        }

        public List<CellFragment> CellFragments { get; set; }

        private CellStatement cellState;
        public CellStatement CellState {
            get {
                return cellState;
            }
            set {
                cellState = value;
                NotifyPropertyChanging(nameof(CellState));
            }
        }

        public ObjectSectorCell() {
            CellFragments = new List<CellFragment>();
        }
    }
}
