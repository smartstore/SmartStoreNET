using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class BankConnectionSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the bank name that will be used
        /// </summary>
        public string Bankname { get; set; }

        /// <summary>
        /// Gets or sets the bank code that will be used
        /// </summary>
        public string Bankcode { get; set; }

        /// <summary>
        /// Gets or sets the account number that will be used
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the account holder that will be used
        /// </summary>
        public string AccountHolder { get; set; }

        /// <summary>
        /// Gets or sets the iban that will be used
        /// </summary>
        public string Iban { get; set; }

        /// <summary>
        /// Gets or sets the bic that will be used
        /// </summary>
        public string Bic { get; set; }
    }
}