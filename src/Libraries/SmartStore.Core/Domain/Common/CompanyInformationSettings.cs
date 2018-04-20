
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class CompanyInformationSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the company name that will be used
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the salutation that will be used
        /// </summary>
        public string Salutation { get; set; }

        /// <summary>
        /// Gets or sets the title that will be used
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the firstname that will be used
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Gets or sets the lastname that will be used
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// Gets or sets the company management description that will be used
        /// </summary>
        public string CompanyManagementDescription { get; set; }

        /// <summary>
        /// Gets or sets the company management that will be used
        /// </summary>
        public string CompanyManagement { get; set; }

        /// <summary>
        /// Gets or sets the street that will be used
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the housenumber that will be used
        /// </summary>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the zip code that will be used
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the location that will be used
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the country that will be used
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the state that will be used
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the country that will be used
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the state that will be used
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// Gets or sets the vat id that will be used
        /// </summary>
        public string VatId { get; set; }

        /// <summary>
        /// Gets or sets the commercial register that will be used
        /// </summary>
        public string CommercialRegister { get; set; }

        /// <summary>
        /// Gets or sets the tax number that will be used
        /// </summary>
        public string TaxNumber { get; set; }
    }
}