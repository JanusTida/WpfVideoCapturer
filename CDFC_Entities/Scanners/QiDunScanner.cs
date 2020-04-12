using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Scanners {
    public class QiDunScanner: DefaultObjectScanner {
        public QiDunScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.QiDun, iObjectDevice) { }
    }
}
