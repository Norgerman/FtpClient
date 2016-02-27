using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FtpClient
{
    class PortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int i;
            if (!int.TryParse(value.ToString(), out i))
            {
                return new ValidationResult(false, "Port can only be Integer");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
