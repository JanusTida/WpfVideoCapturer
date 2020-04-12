using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Recoverers {
    /// <summary>
    /// 安联恢复器;
    /// </summary>
    public class WFSRecoverer : DefaultObjectRecoverer {
        public WFSRecoverer(ScanMethod scanMethod, IObjectDevice iObjectDevice) : base(DeviceTypeEnum.WFS, scanMethod, iObjectDevice) {
        }
    }
}
