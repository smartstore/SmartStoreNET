using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Common
{
    public class AddressSettings : BaseEntity, ISettings, ILocalizedEntity
    {
		public AddressSettings()
		{
            ValidateEmailAddress = false;
            SalutationEnabled = false;
            TitleEnabled = false;
			CompanyEnabled = true;
			StreetAddressEnabled = true;
			StreetAddressRequired = true;
			StreetAddress2Enabled = true;
			ZipPostalCodeEnabled = true;
			ZipPostalCodeRequired = true;
			CityEnabled = true;
			CityRequired = true;
			CountryEnabled = true;
			CountryRequired = true;
			StateProvinceEnabled = true;
			StateProvinceRequired = false;
			PhoneEnabled = true;
			PhoneRequired = true;
			FaxEnabled = true;
		}

        /// <summary>
        /// Gets or sets a value indicating whether email address should be validated
        /// </summary>
        public bool ValidateEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Salutation' is enabled
        /// </summary>
        public bool SalutationEnabled { get; set; }

        /// <summary>
        /// Gets or sets values with available salutations
        /// </summary>
        public string Salutations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Title' is enabled
        /// </summary>
        public bool TitleEnabled { get; set; }

		/// <summary>
        /// Gets or sets a value indicating whether 'Company' is enabled
        /// </summary>
        public bool CompanyEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Company' is required
        /// </summary>
        public bool CompanyRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Street address' is enabled
        /// </summary>
        public bool StreetAddressEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Street address' is required
        /// </summary>
        public bool StreetAddressRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Street address 2' is enabled
        /// </summary>
        public bool StreetAddress2Enabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Street address 2' is required
        /// </summary>
        public bool StreetAddress2Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Zip / postal code' is enabled
        /// </summary>
        public bool ZipPostalCodeEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Zip / postal code' is required
        /// </summary>
        public bool ZipPostalCodeRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'City' is enabled
        /// </summary>
        public bool CityEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'City' is required
        /// </summary>
        public bool CityRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Country' is enabled
        /// </summary>
        public bool CountryEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether 'Country' is required
		/// </summary>
		public bool CountryRequired { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether 'State / province' is enabled
		/// </summary>
		public bool StateProvinceEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether 'State / province' is required
		/// </summary>
		public bool StateProvinceRequired { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether 'Phone number' is enabled
		/// </summary>
		public bool PhoneEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Phone number' is required
        /// </summary>
        public bool PhoneRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Fax number' is enabled
        /// </summary>
        public bool FaxEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'Fax number' is required
        /// </summary>
        public bool FaxRequired { get; set; }
    }
}