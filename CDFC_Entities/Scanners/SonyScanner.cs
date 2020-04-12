using CDFCEntities.Abstracts;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class SonyScanner:DefaultObjectScanner {
        public SonyScanner(IObjectDevice iObjectDevice) : base(Enums.DeviceTypeEnum.Sony,iObjectDevice) { }
    }
}
