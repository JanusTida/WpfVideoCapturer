using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Recoverers {
    public class QiDunRecoverer:DefaultObjectRecoverer {
        public QiDunRecoverer(ScanMethod scanMethod, IObjectDevice iObjectDevice) :
            base(DeviceTypeEnum.QiDun,  scanMethod, iObjectDevice) {
        }
    }
}
