using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCValidaters.Validaters {
    public class PhoneRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            var val = value.ToString();
            if(val.All(p => p >= '0'&& p <= '9')&&val.Length>=8&&val.Length<14) {
                return new ValidationResult(true, null);
            }
            else {
                return new ValidationResult(false, FindResourceString("InputValidPhone"));
            }
        }
    }
}
