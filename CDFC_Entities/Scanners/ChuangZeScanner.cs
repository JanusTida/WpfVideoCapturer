using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class ChuangZeScanner : DefaultObjectScanner {
        public ChuangZeScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.ChuangZe, iObjectDevice) { }
    }
}
