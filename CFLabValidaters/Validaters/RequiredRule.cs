using System.Globalization;
using System.Windows.Controls;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCValidaters.Validaters {
    public class RequiredRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value == null)
                return new ValidationResult(false,FindResourceString("ValueCannotBeNull"));
            if (string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false,FindResourceString("ValueCannotBeNull"));
            return new ValidationResult(true, null);
        }
    }
}
