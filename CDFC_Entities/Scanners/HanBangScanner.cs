using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class HanBangScanner : DefaultObjectScanner {
        public HanBangScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.HanBang, iObjectDevice) { }

    }
    
}
