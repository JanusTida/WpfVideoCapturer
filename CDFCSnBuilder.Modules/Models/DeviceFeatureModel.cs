using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCSnBuilder.Modules.Models {
    public class DeviceFeatureModel:BindableBase {
        public string Name { get; set; }
        public int FeatureID { get; set; }

        private bool isChecked;
        public bool IsChecked {
            get {
                return isChecked;
            }
            set {
                SetProperty(ref isChecked, value);
                IsCheckedChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler IsCheckedChanged;
    }
}
