using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Core.Domain.Customers
{
    public class PrivacySettings : BaseEntity, ISettings, ILocalizedEntity
	{
		public PrivacySettings()
		{
			EnableCookieConsent = true;
		}

		/// <summary>
		/// Specifies whether cookie hint and consent will be displayed to customers in the frontent 
		/// </summary>
		public bool EnableCookieConsent { get; set; }

		public string CookieConsentBadgetext { get; set; }
	}
}