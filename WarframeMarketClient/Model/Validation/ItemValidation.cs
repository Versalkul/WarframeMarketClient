using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace WarframeMarketClient.Model
{
    class ItemValidation:ValidationRule

    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup group = (BindingGroup)value;

            StringBuilder error = null;
            foreach (var item in group.Items)
            {
                // aggregate errors
                IDataErrorInfo info = item as IDataErrorInfo;
                if (info != null)
                {
                    if (!string.IsNullOrEmpty(info.Error))
                    {
                        if (error == null)
                            error = new StringBuilder();
                        error.Append((error.Length != 0 ? ", " : "") + info.Error);
                    }
                }
            }

            if (error != null)
                return new ValidationResult(false, error.ToString());

            return ValidationResult.ValidResult;
        }

        /// <summary>

        /// Validates the proposed value.

        /// </summary>

        /// <param name="value">The proposed value.</param>

        /// <param name="cultureInfo">A CultureInfo.</param>

        /// <returns>The result of the validation.</returns>



    }

}