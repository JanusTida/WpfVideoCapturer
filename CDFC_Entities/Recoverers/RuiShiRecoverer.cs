using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Recoverers {
    /// <summary>
    /// 安联恢复器;
    /// </summary>
    public class RuiShiRecoverer : DefaultObjectRecoverer {
        public RuiShiRecoverer(ScanMethod scanMethod, IObjectDevice iObjectDevice) : base(DeviceTypeEnum.RuiShi, scanMethod, iObjectDevice) {
        }
    }
}
