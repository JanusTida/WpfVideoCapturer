using CDFCEntities.Abstracts;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Scanners
{
    public class ShanLingScanner:DefaultObjectScanner
    {
        public ShanLingScanner(IObjectDevice iObjectDevice) : base(Enums.DeviceTypeEnum.ShanLing, iObjectDevice) { }
    }
}
