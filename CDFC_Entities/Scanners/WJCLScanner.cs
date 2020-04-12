using CDFCEntities.Abstracts;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Scanners {
    public class WJCLScanner:DefaultObjectScanner {
        public WJCLScanner(IObjectDevice oDevice):base(Enums.DeviceTypeEnum.WJCL,oDevice) {
                
        }
    }
}
