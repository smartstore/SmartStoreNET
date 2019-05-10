using FluentValidation;
using FluentValidation.Validators;

namespace SmartStore.Web.Framework.Validators
{
    public static class FluentValidatorExtensions
	{
        public static IRuleBuilderOptions<T, string> CreditCardCvvNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new CreditCardCvvNumberValidator());
        }
    }

	public class CreditCardCvvNumberValidator : RegularExpressionValidator
	{
		public CreditCardCvvNumberValidator()
			: base(@"^[0-9]{3,4}$")
		{
		}
	}
}
