using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCDongleChecker.Models {
    //期限制许可类型;
    public class TrailLicenseItem:LicenseItemBase {
        public TrailLicenseItem() : base("Trial") {

        }
        public DateTime StartTime { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
