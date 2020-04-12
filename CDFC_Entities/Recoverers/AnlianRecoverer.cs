using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Recoverers {
    /// <summary>
    /// 安联恢复器;
    /// </summary>
    public class AnlianRecoverer : DefaultObjectRecoverer {
        public AnlianRecoverer(ScanMethod scanMethod, IObjectDevice iObjectDevice) : base(DeviceTypeEnum.AnLian, scanMethod, iObjectDevice) {
        }
    }
}
