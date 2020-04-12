using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Scanners {
    public class UnknownCarScanner:DefaultObjectScanner {
        public UnknownCarScanner(IObjectDevice objectDevice):base(DeviceTypeEnum.UnknownCar,objectDevice) {

        }
    }
}
