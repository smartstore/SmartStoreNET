using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Payments
{
	public class PaymentMethodEditModel : EntityModelBase, ILocalizedModel<PaymentMethodLocalizedModel>
	{
		public PaymentMethodEditModel()
		{
			Locales = new List<PaymentMethodLocalizedModel>();
		}

		public IList<PaymentMethodLocalizedModel> Locales { get; set; }
		public string IconUrl { get; set; }

		[SmartResourceDisplayName("Common.SystemName")]
		public string SystemName { get; set; }

		[SmartResourceDisplayName("Common.FriendlyName")]
		public string FriendlyName { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ExcludedCustomerRole")]
		public string[] ExcludedCustomerRoleIds { get; set; }
		public List<SelectListItem> AvailableCustomerRoles { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ExcludedShippingMethod")]
		public string[] ExcludedShippingMethodIds { get; set; }
		public List<SelectListItem> AvailableShippingMethods { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ExcludedCountry")]
		public string[] ExcludedCountryIds { get; set; }
		public List<SelectListItem> AvailableCountries { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Restrictions.CountryExclusionContext")]
		public CountryRestrictionContextType CountryExclusionContext { get; set; }
		public List<SelectListItem> AvailableCountryExclusionContextTypes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.MinimumOrderAmount")]
		public decimal? MinimumOrderAmount { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.MaximumOrderAmount")]
		public decimal? MaximumOrderAmount { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Restrictions.AmountRestrictionContext")]
		public AmountRestrictionContextType AmountRestrictionContext { get; set; }
		public List<SelectListItem> AvailableAmountRestrictionContextTypes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ShortDescription")]
		[AllowHtml]
		public string Description { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.FullDescription")]
		[AllowHtml]
		public string FullDescription { get; set; }
	}


	public class PaymentMethodLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[SmartResourceDisplayName("Common.FriendlyName")]
		public string FriendlyName { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ShortDescription")]
		[AllowHtml]
		public string Description { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.FullDescription")]
		[AllowHtml]
		public string FullDescription { get; set; }
	}
}