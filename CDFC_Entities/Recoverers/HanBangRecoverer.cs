﻿using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Recoverers {
    /// <summary>
    /// 安联恢复器;
    /// </summary>
    public class HanBangRecoverer : DefaultObjectRecoverer {
        public HanBangRecoverer(ScanMethod scanMethod, IObjectDevice iObjectDevice) : base(DeviceTypeEnum.HanBang, scanMethod, iObjectDevice) {
        }
    }
}
