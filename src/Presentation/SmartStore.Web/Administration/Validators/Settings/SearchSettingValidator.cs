using System;
using System.Linq;
using FluentValidation;
using SmartStore.Admin.Models.Settings;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.Admin.Validators.Settings
{
	public class SearchSettingValidator : SmartValidatorBase<SearchSettingsModel>
	{
		private const int MAX_INSTANT_SEARCH_ITEMS = 16;

		public SearchSettingValidator(ILocalizationService localize, Func<string, bool> addRule)
		{
			if (addRule("InstantSearchNumberOfProducts"))
			{
				RuleFor(x => x.InstantSearchNumberOfProducts)
					.Must(x => x >= 1 && x <= MAX_INSTANT_SEARCH_ITEMS)
					.When(x => x.InstantSearchEnabled)
					.WithMessage(localize.GetResource("Admin.Validation.ValueRange").FormatInvariant(1, MAX_INSTANT_SEARCH_ITEMS));
			}

			if (addRule("CommonFacets"))
			{
				RuleFor(x => x.CommonFacets)
					.Must(x => x.GroupBy(y => SeoExtensions.GetSeName(y.Alias)).Where(y => y.Key.HasValue() && y.Count() > 1).FirstOrDefault() == null)
					.WithMessage(localize.GetResource("Common.Error.AliasAlreadyExists"), 
						x => x.CommonFacets.GroupBy(y => SeoExtensions.GetSeName(y.Alias)).Where(y => y.Key.HasValue() && y.Count() > 1).FirstOrDefault()?.Key);
			}
		}
	}
}