using CDFCEntities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;

namespace CDFCEntities.Scanners {
    public class JVSScanner : DefaultObjectScanner {
        public JVSScanner( IObjectDevice iObjectDevice) : base( DeviceTypeEnum.JVS, iObjectDevice) {
        }
    }
}
