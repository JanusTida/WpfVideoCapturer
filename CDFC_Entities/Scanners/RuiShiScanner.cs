using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class RuiShiScanner : DefaultObjectScanner {
        public RuiShiScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.RuiShi, iObjectDevice) { }
    }
}
