using CDFCConverter.Enums;
using CDFCEntities.Files;

namespace CDFCVideoExactor.Models {
    //碎片节点的碎片图表表示;
    public class CellFragment {
        //碎片节点本体;
        public FileFragment Fragment { get; set; }
        //碎片节点状态;
        public CellStatement FragmentStatement { get; set; }
    }
}
