using CDFCEntities.Abstracts;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Scanners {
    public class LingDuScanner:DefaultObjectScanner {
        public LingDuScanner(IObjectDevice iObjectDevice) : base(Enums.DeviceTypeEnum.LingDu, iObjectDevice) { }
    }
}
