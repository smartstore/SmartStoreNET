using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;

namespace SmartStore.Services.Messages
{
    public partial class EmailAccountService:IEmailAccountService
    {
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="emailAccountRepository">Email account repository</param>
        /// <param name="emailAccountSettings"></param>
        /// <param name="eventPublisher">Event published</param>
        public EmailAccountService(IRepository<EmailAccount> emailAccountRepository,
            EmailAccountSettings emailAccountSettings, IEventPublisher eventPublisher)
        {
            _emailAccountRepository = emailAccountRepository;
            _emailAccountSettings = emailAccountSettings;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Inserts an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual void InsertEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

			emailAccount.Email = emailAccount.Email.EmptyNull();
			emailAccount.DisplayName = emailAccount.DisplayName.EmptyNull();
			emailAccount.Host = emailAccount.Host.EmptyNull();
			emailAccount.Username = emailAccount.Username.EmptyNull();
			emailAccount.Password = emailAccount.Password.EmptyNull();

            emailAccount.Email = emailAccount.Email.Trim();
            emailAccount.DisplayName = emailAccount.DisplayName.Trim();
            emailAccount.Host = emailAccount.Host.Trim();
            emailAccount.Username = emailAccount.Username.Trim();
            emailAccount.Password = emailAccount.Password.Trim();

			emailAccount.Email = emailAccount.Email.Truncate(255);
			emailAccount.DisplayName = emailAccount.DisplayName.Truncate(255);
			emailAccount.Host = emailAccount.Host.Truncate(255);
			emailAccount.Username = emailAccount.Username.Truncate(255);
			emailAccount.Password = emailAccount.Password.Truncate(255);

            _emailAccountRepository.Insert(emailAccount);

            //event notification
            _eventPublisher.EntityInserted(emailAccount);
        }

        /// <summary>
        /// Updates an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual void UpdateEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

			emailAccount.Email = emailAccount.Email.EmptyNull();
			emailAccount.DisplayName = emailAccount.DisplayName.EmptyNull();
			emailAccount.Host = emailAccount.Host.EmptyNull();
			emailAccount.Username = emailAccount.Username.EmptyNull();
			emailAccount.Password = emailAccount.Password.EmptyNull();

            emailAccount.Email = emailAccount.Email.Trim();
            emailAccount.DisplayName = emailAccount.DisplayName.Trim();
            emailAccount.Host = emailAccount.Host.Trim();
            emailAccount.Username = emailAccount.Username.Trim();
            emailAccount.Password = emailAccount.Password.Trim();

			emailAccount.Email = emailAccount.Email.Truncate(255);
			emailAccount.DisplayName = emailAccount.DisplayName.Truncate(255);
			emailAccount.Host = emailAccount.Host.Truncate(255);
			emailAccount.Username = emailAccount.Username.Truncate(255);
			emailAccount.Password = emailAccount.Password.Truncate(255);

            _emailAccountRepository.Update(emailAccount);

            //event notification
            _eventPublisher.EntityUpdated(emailAccount);
        }

        /// <summary>
        /// Deletes an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual void DeleteEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            if (GetAllEmailAccounts().Count == 1)
                throw new SmartException("You cannot delete this email account. At least one account is required.");

            _emailAccountRepository.Delete(emailAccount);

            //event notification
            _eventPublisher.EntityDeleted(emailAccount);
        }

        /// <summary>
        /// Gets an email account by identifier
        /// </summary>
        /// <param name="emailAccountId">The email account identifier</param>
        /// <returns>Email account</returns>
        public virtual EmailAccount GetEmailAccountById(int emailAccountId)
        {
            if (emailAccountId == 0)
                return null;
            
            var emailAccount = _emailAccountRepository.GetById(emailAccountId);
            return emailAccount;
        }

        /// <summary>
        /// Gets all email accounts
        /// </summary>
        /// <returns>Email accounts list</returns>
        public virtual IList<EmailAccount> GetAllEmailAccounts()
        {
            var query = from ea in _emailAccountRepository.Table
                        orderby ea.Id
                        select ea;
            var emailAccounts = query.ToList();
            return emailAccounts;
        }
    }
}
