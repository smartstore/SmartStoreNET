using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework.Localization
{
    /// <summary>
    /// Attribute which determines and sets working culture and globalization scripts
    /// </summary>
    public class SetWorkingCultureAttribute : FilterAttribute, IAuthorizationFilter, IActionFilter
    {
		public Lazy<IWorkContext> WorkContext { get; set; }
		public Lazy<IPageAssetsBuilder> AssetBuilder { get; set; }

		public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext?.HttpContext?.Request;
            if (request == null)
                return;

			if (filterContext.IsChildAction)
				return;

			if (!DataSettings.DatabaseIsInstalled())
                return;

            var workContext = WorkContext.Value;

            CultureInfo culture = workContext.CurrentCustomer != null && workContext.WorkingLanguage != null
				? new CultureInfo(workContext.WorkingLanguage.LanguageCulture)
				: new CultureInfo("en-US");

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext.IsChildAction)
				return;

			if (!DataSettings.DatabaseIsInstalled())
				return;

			if (!(filterContext.Result is ViewResult))
				return;

			var culture = Thread.CurrentThread.CurrentUICulture;
			if (culture.Name == "en-US")
				return;

			var builder = AssetBuilder.Value;
			var json = CreateCultureJson(culture);
			
			var sb = new StringBuilder();
			sb.Append("<script>");
			sb.Append("jQuery(function () { if (SmartStore.globalization) { SmartStore.globalization.culture = ");
			sb.Append(json);
			sb.Append("; }; });");
			sb.Append("</script>");

			var script = sb.ToString();

			builder.AppendCustomHeadParts(script);
		}

		private string CreateCultureJson(CultureInfo ci)
		{
			var nf = ci.NumberFormat;
			var df = ci.DateTimeFormat;

			var dict = new Dictionary<string, object>
			{
				{ "name", ci.Name },
				{ "englishName", ci.EnglishName },
				{ "nativeName", ci.NativeName },
				{ "isRTL", WorkContext.Value.WorkingLanguage?.Rtl ?? ci.TextInfo.IsRightToLeft }, // favor RTL property of Language
				{ "language", ci.TwoLetterISOLanguageName },
				{ "numberFormat", new Dictionary<string, object>
				{
					{ ",", nf.NumberGroupSeparator },
					{ ".", nf.NumberDecimalSeparator },
					{ "pattern", new[] { nf.NumberNegativePattern } },
					{ "decimals", nf.NumberDecimalDigits },
					{ "groupSizes", nf.NumberGroupSizes },
					{ "+", nf.PositiveSign },
					{ "-", nf.NegativeSign },
					{ "NaN", nf.NaNSymbol },
					{ "negativeInfinity", nf.NegativeInfinitySymbol },
					{ "positiveInfinity", nf.PositiveInfinitySymbol },
					{ "percent", new Dictionary<string, object>
					{
						{ ",", nf.PercentGroupSeparator },
						{ ".", nf.PercentDecimalSeparator },
						{ "pattern", new[] { nf.PercentNegativePattern, nf.PercentPositivePattern } },
						{ "decimals", nf.PercentDecimalDigits },
						{ "groupSizes", nf.PercentGroupSizes },
						{ "symbol", nf.PercentSymbol }
					} },
					{ "currency", new Dictionary<string, object>
					{
						{ ",", nf.CurrencyGroupSeparator },
						{ ".", nf.CurrencyDecimalSeparator },
						{ "pattern", new[] { nf.CurrencyNegativePattern, nf.CurrencyPositivePattern } },
						{ "decimals", nf.CurrencyDecimalDigits },
						{ "groupSizes", nf.CurrencyGroupSizes },
						{ "symbol", nf.CurrencySymbol }
					} },
				} },
				{ "dateTimeFormat", new Dictionary<string, object>
				{
					{ "calendarName", df.NativeCalendarName },
					{ "/", df.DateSeparator },
					{ ":", df.TimeSeparator },
					{ "firstDay", (int)df.FirstDayOfWeek },
					{ "twoDigitYearMax", ci.Calendar.TwoDigitYearMax },
					{ "AM", df.AMDesignator.IsEmpty() ? null : new[] { df.AMDesignator, df.AMDesignator.ToLower(), df.AMDesignator.ToUpper() } },
					{ "PM", df.PMDesignator.IsEmpty() ? null : new[] { df.PMDesignator, df.PMDesignator.ToLower(), df.PMDesignator.ToUpper() } },
					{ "days", new Dictionary<string, object>
					{
						{ "names", df.DayNames },
						{ "namesAbbr", df.AbbreviatedDayNames },
						{ "namesShort", df.ShortestDayNames },
					} },
					{ "months", new Dictionary<string, object>
					{
						{ "names", df.MonthNames },
						{ "namesAbbr", df.AbbreviatedMonthNames },
					} },
					{ "patterns", new Dictionary<string, object>
					{
						{ "d", df.ShortDatePattern },
						{ "D", df.LongDatePattern },
						{ "t", df.ShortTimePattern },
						{ "T", df.LongTimePattern },
						{ "g", df.ShortDatePattern + " " + df.ShortTimePattern },
						{ "G", df.ShortDatePattern + " " + df.LongTimePattern },
						{ "f", df.FullDateTimePattern }, // TODO: (mc) find it actually
						{ "F", df.FullDateTimePattern },
						{ "M", df.MonthDayPattern },
						{ "Y", df.YearMonthPattern },
						{ "u", df.UniversalSortableDateTimePattern },
					} }
				} }
			};

			var json = JsonConvert.SerializeObject(dict, new JsonSerializerSettings
			{
				Formatting = Formatting.None
			});

			return json;
		}
	}
}
