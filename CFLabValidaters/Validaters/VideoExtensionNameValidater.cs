using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace CDFCValidaters.Validaters {
    public class VideoExtensionNameValidater : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string extensionValue = value.ToString();
            bool res;
            if (!extensionValue.StartsWith(".")) {
                res = false;
            }
            else if(extensionValue.Count(p =>p == '.') > 1) {
                res = false;
            }else if (extensionValue.Contains("$")) {
                res = false;
            }else {
                res = true;
            }
            return new ValidationResult(res, res ? null : "Error Extension Name!");
        }
    }
}
