using CDFCEntities.Abstracts;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDFCEntities.Enums;

namespace CDFCEntities.Scanners {
    public class XiaoMiScanner:DefaultObjectScanner {
        public XiaoMiScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.XiaoMi,iObjectDevice) { }
    }
}
