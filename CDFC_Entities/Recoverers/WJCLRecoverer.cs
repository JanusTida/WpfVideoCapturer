﻿using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Recoverers {
    public class WJCLRecoverer:DefaultObjectRecoverer {
        public WJCLRecoverer(ScanMethod scanMethod, IObjectDevice oDevice) :base(Enums.DeviceTypeEnum.WJCL,scanMethod,oDevice){
                
        }
    }
}
