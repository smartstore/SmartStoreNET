using FluentValidation;

namespace SmartStore.Web.Framework.Validators
{
    public static class MyValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> IsCreditCard<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new CreditCardPropertyValidator());
        }
    }
}
