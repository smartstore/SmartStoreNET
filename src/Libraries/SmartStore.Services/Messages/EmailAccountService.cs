using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Messages
{
    public partial class EmailAccountService : IEmailAccountService
    {
		private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEventPublisher _eventPublisher;

		private EmailAccount _defaultEmailAccount;

        public EmailAccountService(
			IRepository<EmailAccount> emailAccountRepository, 
			EmailAccountSettings emailAccountSettings, 
			IEventPublisher eventPublisher)
        {
            this._emailAccountRepository = emailAccountRepository;
            this._emailAccountSettings = emailAccountSettings;
            this._eventPublisher = eventPublisher;
        }

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

			_defaultEmailAccount = null;
        }

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

			_defaultEmailAccount = null;
        }

        public virtual void DeleteEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            if (GetAllEmailAccounts().Count == 1)
                throw new SmartException("You cannot delete this email account. At least one account is required.");

            _emailAccountRepository.Delete(emailAccount);

			_defaultEmailAccount = null;
        }

        public virtual EmailAccount GetEmailAccountById(int emailAccountId)
        {
            if (emailAccountId == 0)
                return null;

			return _emailAccountRepository.GetByIdCached(emailAccountId, "db.emailaccount.id-" + emailAccountId);
		}

		public virtual EmailAccount GetDefaultEmailAccount()
		{
			if (_defaultEmailAccount == null)
			{
				_defaultEmailAccount = GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
				if (_defaultEmailAccount == null)
				{
					_defaultEmailAccount = GetAllEmailAccounts().FirstOrDefault();
				}
			}

			return _defaultEmailAccount;
		}

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
