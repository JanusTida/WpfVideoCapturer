using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class MP4Scanner : DefaultObjectScanner {
        public MP4Scanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.H264, iObjectDevice) { }
    }
}
