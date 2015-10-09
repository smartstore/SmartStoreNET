using System.Collections.Generic;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
    public partial interface IEmailAccountService
    {
        /// <summary>
        /// Inserts an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        void InsertEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Updates an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        void UpdateEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Deletes an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        void DeleteEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Gets an email account by identifier
        /// </summary>
        /// <param name="emailAccountId">The email account identifier</param>
        /// <returns>Email account</returns>
        EmailAccount GetEmailAccountById(int emailAccountId);

		/// <summary>
		/// Gets the default email account
		/// </summary>
		/// <returns>Emil account</returns>
		EmailAccount GetDefaultEmailAccount();

        /// <summary>
        /// Gets all email accounts
        /// </summary>
        /// <returns>Email accounts list</returns>
        IList<EmailAccount> GetAllEmailAccounts();
    }
}
