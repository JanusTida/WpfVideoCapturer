using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class MOVScanner :DefaultObjectScanner{
        public MOVScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.MOV,iObjectDevice) { }
    }
}
