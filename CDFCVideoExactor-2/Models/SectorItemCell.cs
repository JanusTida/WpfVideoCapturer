using CDFCConverter.Enums;
using CDFCVideoExactor.Abstracts;
using System;

namespace CDFCVideoExactor.Models {
    //碎片单位;
    //扇区单位;
    public class SectorItemCell : NotificationObject {
        public ulong SectorAddress { get; set; }
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
        public CellFragment CellFragment { get; set; }
    }
}
