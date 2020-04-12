using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class HSTScanner : DefaultObjectScanner {
        public HSTScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.HaiShiTai, iObjectDevice) { }
    }
}
