using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Recoverers {
    public class MOVRecoverer:DefaultObjectRecoverer {
        public MOVRecoverer(ScanMethod scanMethod, IObjectDevice iObjectDevice) : base(Enums.DeviceTypeEnum.MOV, scanMethod, iObjectDevice) {

        }
    }
}
