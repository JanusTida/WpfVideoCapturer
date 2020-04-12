using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Recoverers {
    /// <summary>
    /// 安联恢复器;
    /// </summary>
    public class ChuangZeRecoverer : DefaultObjectRecoverer {
        public ChuangZeRecoverer(ScanMethod scanMethod, IObjectDevice iObjectDevice) : base(DeviceTypeEnum.ChuangZe, scanMethod, iObjectDevice) {
        }
    }
}
