using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class HaiSiScanner : DefaultObjectScanner {
        public HaiSiScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.HaiSi, iObjectDevice) { }
    }
}
