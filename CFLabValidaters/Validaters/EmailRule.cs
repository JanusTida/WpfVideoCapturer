using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCValidaters.Validaters {
    public class EmailRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if(value != null) {
                return ValidateEmail(value.ToString());
            }
            return new ValidationResult(false,FindResourceString("InputEmail"));
        }

        public static ValidationResult ValidateEmail(string strValue) {
            string expression = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            if (Regex.IsMatch(strValue, expression, RegexOptions.Compiled))
                return new ValidationResult(true, null); //验证OK
            else
                return new ValidationResult(false,FindResourceString("InputValidEmail")); //验证失败
        }
    }
}
