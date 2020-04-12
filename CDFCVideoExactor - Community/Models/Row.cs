using System.Collections.ObjectModel;

namespace CDFCVideoExactor.Models {
    //碎片行;
    public class Row {
        public ObservableCollection<ObjectSectorCell> Cells { get; private set; }
        public Row() {
            Cells = new ObservableCollection<ObjectSectorCell>();
        }

    }
}
