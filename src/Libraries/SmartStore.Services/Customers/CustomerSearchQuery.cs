using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Customers
{
    public class CustomerSearchQuery : ICloneable<CustomerSearchQuery>
    {
        /// <summary>
        /// Customer registration from; null to load all customers. Default: null.
        /// </summary>
        public DateTime? RegistrationFromUtc { get; set; }

        /// <summary>
        /// Customer registration to; null to load all customers. Default: null.
        /// </summary>
        public DateTime? RegistrationToUtc { get; set; }

        /// <summary>
        /// Customer last activity date (from)s. Default: null.
        /// </summary>
        public DateTime? LastActivityFromUtc { get; set; }

        /// <summary>
        /// A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers. Default: null.
        /// </summary>
        public int[] CustomerRoleIds { get; set; }

        /// <summary>
        /// Affiliate identifie. Default: null.
        /// </summary>
        public int? AffiliateId { get; set; }

        /// <summary>
        /// Email. Default: null.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// UserName. Default: null.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Phone. Default: null.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// ZipPostalCode. Default: null.
        /// </summary>
        public string ZipPostalCode { get; set; }

        /// <summary>
        /// CustomerNumber. Default: null.
        /// </summary>
        public string CustomerNumber { get; set; }

        /// <summary>
        /// Day of birth. Default: null.
        /// </summary>
        public int? DayOfBirth { get; set; }

        /// <summary>
        /// Month of birth. Default: null.
        /// </summary>
        public int? MonthOfBirth { get; set; }

        /// <summary>
        /// Password format. Default: null.
        /// </summary>
        public PasswordFormat? PasswordFormat { get; set; }

        /// <summary>
        /// Whether to only load customers with shopping cart. Default: false (meaning: no matter)
        /// </summary>
        public bool OnlyWithCart { get; set; }

        /// <summary>
        /// What shopping cart type to filter; used when 'HasCart' param is 'true'. Default: null.
        /// </summary>
        public ShoppingCartType? CartType { get; set; }

        /// <summary>
        /// Whether only (soft)-deleted records should be loaded. Default: false (meaning: only undeleted)
        /// </summary>
        public bool? Deleted { get; set; } = false;

        /// <summary>
        /// Whether only active customers should be loaded.
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// Whether only system account records should be loaded. Default: false (meaning: ignore system accounts)
        /// </summary>
        public bool? IsSystemAccount { get; set; } = false;

        /// <summary>
        /// Searches in FullName (FirstName + LastName) and Company fields. Default: null.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Page index. Default: 0.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page index. Default: 50.
        /// </summary>
        public int PageSize { get; set; } = 50;

        public CustomerSearchQuery Clone()
        {
            return (CustomerSearchQuery)this.MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
