using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCSnBuilder.Modules.Models {
    public class BuiltSnModel:BindableBase {
        public string FeatureID { get; set; }
        public string DateTime { get; set; }
        public TimeSpan? LimTS { get; set; }
        public string Sn { get; set; }
    }
}
