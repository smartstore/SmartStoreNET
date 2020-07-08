using System;
using System.Globalization;
using System.Linq;
using SmartStore.Core.Domain.Localization;
using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class WeekdayRule : ListRuleBase<int>
    {
        public static RuleValueSelectListOption[] GetDefaultValues(Language language)
        {
            CultureInfo cultureInfo = null;

            try
            {
                cultureInfo = CultureInfo.GetCultureInfo(language.LanguageCulture);
            }
            catch { }

            var dtif = cultureInfo?.DateTimeFormat ?? DateTimeFormatInfo.InvariantInfo;

            var options = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(x => new RuleValueSelectListOption { Value = ((int)x).ToString(), Text = dtif.GetDayName(x) })
                .ToArray();

            return options;
        }

        protected override int GetValue(CartRuleContext context)
        {
            return (int)DateTime.Now.DayOfWeek;
        }
    }
}
