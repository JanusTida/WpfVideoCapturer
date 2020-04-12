using CDFCEntities.Abstracts;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Scanners {
    public class YinTanScanner:DefaultObjectScanner {
        public YinTanScanner(IObjectDevice device):base(Enums.DeviceTypeEnum.YinTan,device) {

        }
    }
}
