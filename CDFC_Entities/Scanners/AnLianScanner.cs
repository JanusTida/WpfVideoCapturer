using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class AnLianScanner : DefaultObjectScanner {
        public AnLianScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.AnLian,iObjectDevice) { }
    }
}
