using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCDongleChecker.Models {
    //许可基类;
    public class LicenseItemBase {
        public LicenseItemBase(string licenseType) {
            this.LicenseType = licenseType;
        }
        private string licenseType;
        public string LicenseType {
            get {
                if(licenseType == "Perputal") {
                    return "永久许可";
                }
                else if(licenseType == "Trial"){
                    return "期限许可";
                }
                else {
                    return "未录入";
                }
            }
            private set {
                licenseType = value;
            }
        }

        public string ModuleName {
            get;set;
        }
    }
}
