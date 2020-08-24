using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Customers
{
	public class PrivacySettings : BaseEntity, ISettings, ILocalizedEntity
	{
		public PrivacySettings()
		{
			EnableCookieConsent = true;
			ModalCookieConsent = true;
			StoreLastIpAddress = false;
			DisplayGdprConsentOnForms = true;
			FullNameOnContactUsRequired = false;
		}

		/// <summary>
		/// Specifies whether cookie hint and consent will be displayed to customers in the frontent 
		/// </summary>
		public bool EnableCookieConsent { get; set; }

		/// <summary>
		/// Specifies whether the cookie dialog will be display in a modal dialog
		/// </summary>
		public bool ModalCookieConsent { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to store last IP address for each customer
		/// </summary>
		public bool StoreLastIpAddress { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to display a checkbox to the customer where he can agree to privacy terms
		/// </summary>
		public bool DisplayGdprConsentOnForms { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the full name field is required on contact us requests
		/// </summary>
		public bool FullNameOnContactUsRequired { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the full name field is required on product requests
		/// </summary>
		public bool FullNameOnProductRequestRequired { get; set; }

		/// <summary>
		/// Gets or sets cookie infos (JSON serialized)
		/// </summary>
		public string CookieInfos { get; set; }
	}
}