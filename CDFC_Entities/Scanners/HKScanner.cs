using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class HKScanner : DefaultObjectScanner {
        public HKScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.HaiKang, iObjectDevice) { }
    }
}
