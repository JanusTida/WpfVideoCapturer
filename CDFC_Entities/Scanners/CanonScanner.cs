using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class CanonScanner: DefaultObjectScanner {
        public CanonScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.Canon, iObjectDevice) { }
        
    }
}
