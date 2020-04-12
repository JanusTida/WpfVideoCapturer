using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Recoverers {
    /// <summary>
    /// 安联恢复器;
    /// </summary>
    public class HaiSiRecoverer : DefaultObjectRecoverer {
        public HaiSiRecoverer(ScanMethod scanMethod, IObjectDevice iObjectDevice) : base(DeviceTypeEnum.HaiSi, scanMethod, iObjectDevice) {
        }
    }
}
