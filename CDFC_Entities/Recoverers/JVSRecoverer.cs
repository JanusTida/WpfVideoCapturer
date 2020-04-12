using CDFCEntities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Recoverers {
    public class JVSRecoverer : DefaultObjectRecoverer {
        public JVSRecoverer( ScanMethod scanMethod, IObjectDevice iObjectDevice) : base(DeviceTypeEnum.JVS, scanMethod, iObjectDevice) {
        }
    }
}
