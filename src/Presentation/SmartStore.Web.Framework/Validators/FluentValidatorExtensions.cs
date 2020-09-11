using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Localization;

namespace SmartStore.Web.Framework.Validators
{
    public static class FluentValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> CreditCardCvvNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new CreditCardCvvNumberValidator());
        }

        public static IRuleBuilderOptions<T, string> Password<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Localizer localizer,
            int minLength = 4,
            int maxLength = 500,
            int minDigits = 0,
            int minSpecialChars = 0,
            int minUppercaseChars = 0)
        {
            var options = ruleBuilder
                .NotEmpty()
                .Length(minLength, maxLength)
                .Must((obj, val, ctx) =>
                {
                    var isValid = true;

                    if (minDigits > 0 || minSpecialChars > 0 || minUppercaseChars > 0)
                    {
                        isValid =
                            val.Count(y => char.IsDigit(y)) >= minDigits &&
                            val.Count(y => !char.IsLetterOrDigit(y)) >= minSpecialChars &&
                            val.Count(y => char.IsUpper(y)) >= minUppercaseChars;

                        var messageArgs = new List<string>();

                        if (minDigits > 0)
                            messageArgs.Add(localizer("Account.Fields.Password.Digits", minDigits));
                        if (minSpecialChars > 0)
                            messageArgs.Add(localizer("Account.Fields.Password.SpecialChars", minSpecialChars));
                        if (minUppercaseChars > 0)
                            messageArgs.Add(localizer("Account.Fields.Password.UppercaseChars", minUppercaseChars));

                        ctx.MessageFormatter.AppendArgument("0", string.Join(", ", messageArgs));
                    }

                    return isValid;
                })
                .WithMessage(localizer("Account.Fields.Password.MustContainChars"));

            return options;
        }

        public static IRuleBuilderOptions<T, string> Password<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Localizer localizer,
            CustomerSettings settings)
        {
            Guard.NotNull(localizer, nameof(localizer));
            Guard.NotNull(settings, nameof(settings));

            return ruleBuilder.Password(
                localizer,
                settings.PasswordMinLength,
                500,
                settings.MinDigitsInPassword,
                settings.MinSpecialCharsInPassword,
                settings.MinUppercaseCharsInPassword);
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
