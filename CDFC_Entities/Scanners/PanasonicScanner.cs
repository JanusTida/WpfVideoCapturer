using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    /// <summary>
    /// 松下的扫描器;
    /// </summary>
    public class PanasonicScanner:DefaultObjectScanner {
        public PanasonicScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.Panasonic,iObjectDevice) { }
    }
}
