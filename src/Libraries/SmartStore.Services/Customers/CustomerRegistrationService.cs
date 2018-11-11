using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;

namespace SmartStore.Services.Customers
{
	/// <summary>
	/// Customer registration service
	/// </summary>
	public partial class CustomerRegistrationService : ICustomerRegistrationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;
		private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public CustomerRegistrationService(ICustomerService customerService, 
            IEncryptionService encryptionService, 
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            RewardPointsSettings rewardPointsSettings, CustomerSettings customerSettings,
            IStoreContext storeContext, IEventPublisher eventPublisher)
        {
            this._customerService = customerService;
            this._encryptionService = encryptionService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._rewardPointsSettings = rewardPointsSettings;
            this._customerSettings = customerSettings;
			this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;

			T = NullLocalizer.Instance;
		}

		#endregion

		public Localizer T { get; set; }

		#region Methods

		/// <summary>
		/// Validate customer
		/// </summary>
		/// <param name="usernameOrEmail">Username or email</param>
		/// <param name="password">Password</param>
		/// <returns>Result</returns>
		public virtual bool ValidateCustomer(string usernameOrEmail, string password)
        {
            Customer customer = null;
            if (_customerSettings.UsernamesEnabled)
                customer = _customerService.GetCustomerByUsername(usernameOrEmail);
            else
                customer = _customerService.GetCustomerByEmail(usernameOrEmail);

            if (customer == null || customer.Deleted || !customer.Active)
                return false;

            //only registered can login
            if (!customer.IsRegistered())
                return false;

            string pwd = "";
            switch (customer.PasswordFormat)
            {
                case PasswordFormat.Encrypted:
                    pwd = _encryptionService.EncryptText(password);
                    break;
                case PasswordFormat.Hashed:
                    pwd = _encryptionService.CreatePasswordHash(password, customer.PasswordSalt, _customerSettings.HashedPasswordFormat);
                    break;
                default:
                    pwd = password;
                    break;
            }

            bool isValid = pwd == customer.Password;

            //save last login date
            if (isValid)
            {
                customer.LastLoginDateUtc = DateTime.UtcNow;
                _customerService.UpdateCustomer(customer);
            }
            //else
            //{
            //    customer.FailedPasswordAttemptCount++;
            //    UpdateCustomer(customer);
            //}

            return isValid;
        }

        /// <summary>
        /// Register customer
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request)
        {
			Guard.ArgumentNotNull(() => request);
			Guard.ArgumentNotNull(() => request.Customer);

            var result = new CustomerRegistrationResult();

            if (request.Customer.IsSearchEngineAccount())
            {
                result.AddError(T("Account.Register.Errors.CannotRegisterSearchEngine"));
                return result;
            }
            if (request.Customer.IsBackgroundTaskAccount())
            {
                result.AddError(T("Account.Register.Errors.CannotRegisterTaskAccount"));
                return result;
            }
            if (request.Customer.IsRegistered())
            {
                result.AddError(T("Account.Register.Errors.AlreadyRegistered"));
                return result;
            }
            if (String.IsNullOrEmpty(request.Email))
            {
                result.AddError(T("Account.Register.Errors.EmailIsNotProvided"));
                return result;
            }
			if (!request.Email.IsEmail())
            {
                result.AddError(T("Common.WrongEmail"));
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError(T("Account.Register.Errors.PasswordIsNotProvided"));
                return result;
            }
            if (_customerSettings.UsernamesEnabled)
            {
                if (String.IsNullOrEmpty(request.Username))
                {
                    result.AddError(T("Account.Register.Errors.UsernameIsNotProvided"));
                    return result;
                }
            }

            //validate unique user
            if (_customerService.GetCustomerByEmail(request.Email) != null)
            {
                result.AddError(T("Account.Register.Errors.EmailAlreadyExists"));
                return result;
            }
            if (_customerSettings.UsernamesEnabled)
            {
                if (_customerService.GetCustomerByUsername(request.Username) != null)
                {
                    result.AddError(T("Account.Register.Errors.UsernameAlreadyExists"));
                    return result;
                }
            }

            //at this point request is valid
            request.Customer.Username = request.Username;
            request.Customer.Email = request.Email;
            request.Customer.PasswordFormat = request.PasswordFormat;

            switch (request.PasswordFormat)
            {
				case PasswordFormat.Clear:
					request.Customer.Password = request.Password;
					break;
				case PasswordFormat.Encrypted:
					request.Customer.Password = _encryptionService.EncryptText(request.Password);
					break;
				case PasswordFormat.Hashed:
					string saltKey = _encryptionService.CreateSaltKey(5);
					request.Customer.PasswordSalt = saltKey;
					request.Customer.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _customerSettings.HashedPasswordFormat);
					break;
            }

            request.Customer.Active = request.IsApproved;

			if (_customerSettings.RegisterCustomerRoleId != 0)
			{
				var customerRole = _customerService.GetCustomerRoleById(_customerSettings.RegisterCustomerRoleId);
				request.Customer.CustomerRoles.Add(customerRole);
			}

			//add to 'Registered' role
			var registeredRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
			if (registeredRole == null)
			{
				throw new SmartException(T("Admin.Customers.CustomerRoles.CannotFoundRole", "Registered"));
			}

            request.Customer.CustomerRoles.Add(registeredRole);

            //remove from 'Guests' role
            var guestRole = request.Customer.CustomerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Guests);
			if (guestRole != null)
			{
				request.Customer.CustomerRoles.Remove(guestRole);
			}

			//Add reward points for customer registration (if enabled)
			if (_rewardPointsSettings.Enabled && _rewardPointsSettings.PointsForRegistration > 0)
			{
				request.Customer.AddRewardPointsHistoryEntry(_rewardPointsSettings.PointsForRegistration, T("RewardPoints.Message.RegisteredAsCustomer"));
			}

            _customerService.UpdateCustomer(request.Customer);
            _eventPublisher.Publish(new CustomerRegisteredEvent { Customer = request.Customer });

            return result;
        }
        
        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual PasswordChangeResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var result = new PasswordChangeResult();
            if (String.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError(T("Account.ChangePassword.Errors.EmailIsNotProvided"));
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError(T("Account.ChangePassword.Errors.PasswordIsNotProvided"));
                return result;
            }

            var customer = _customerService.GetCustomerByEmail(request.Email);
            if (customer == null)
            {
                result.AddError(T("Account.ChangePassword.Errors.EmailNotFound"));
                return result;
            }


            var requestIsValid = false;
            if (request.ValidateRequest)
            {
                //password
                string oldPwd = "";
                switch (customer.PasswordFormat)
                {
                    case PasswordFormat.Encrypted:
                        oldPwd = _encryptionService.EncryptText(request.OldPassword);
                        break;
                    case PasswordFormat.Hashed:
                        oldPwd = _encryptionService.CreatePasswordHash(request.OldPassword, customer.PasswordSalt, _customerSettings.HashedPasswordFormat);
                        break;
                    default:
                        oldPwd = request.OldPassword;
                        break;
                }

                bool oldPasswordIsValid = oldPwd == customer.Password;
                if (!oldPasswordIsValid)
                    result.AddError(T("Account.ChangePassword.Errors.OldPasswordDoesntMatch"));

                if (oldPasswordIsValid)
                    requestIsValid = true;
            }
            else
                requestIsValid = true;


            //at this point request is valid
            if (requestIsValid)
            {
                switch (request.NewPasswordFormat)
                {
                    case PasswordFormat.Clear:
                        {
                            customer.Password = request.NewPassword;
                        }
                        break;
                    case PasswordFormat.Encrypted:
                        {
                            customer.Password = _encryptionService.EncryptText(request.NewPassword);
                        }
                        break;
                    case PasswordFormat.Hashed:
                        {
                            string saltKey = _encryptionService.CreateSaltKey(5);
                            customer.PasswordSalt = saltKey;
                            customer.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey, _customerSettings.HashedPasswordFormat);
                        }
                        break;
                    default:
                        break;
                }
                customer.PasswordFormat = request.NewPasswordFormat;
                _customerService.UpdateCustomer(customer);
            }

            return result;
        }

        /// <summary>
        /// Sets a user email
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="newEmail">New email</param>
        public virtual void SetEmail(Customer customer, string newEmail)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            newEmail = newEmail.Trim();
            string oldEmail = customer.Email;

			if (!newEmail.IsEmail())
                throw new SmartException(T("Account.EmailUsernameErrors.NewEmailIsNotValid"));

            if (newEmail.Length > 100)
                throw new SmartException(T("Account.EmailUsernameErrors.EmailTooLong"));

            var customer2 = _customerService.GetCustomerByEmail(newEmail);
            if (customer2 != null && customer.Id != customer2.Id)
                throw new SmartException(T("Account.EmailUsernameErrors.EmailAlreadyExists"));

            customer.Email = newEmail;
            _customerService.UpdateCustomer(customer);

            //update newsletter subscription (if required)
            if (!String.IsNullOrEmpty(oldEmail) && !oldEmail.Equals(newEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                var subscriptionOld = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(oldEmail, _storeContext.CurrentStore.Id);
                if (subscriptionOld != null)
                {
                    subscriptionOld.Email = newEmail;
                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscriptionOld);
                }
            }
        }

        /// <summary>
        /// Sets a customer username
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="newUsername">New Username</param>
        public virtual void SetUsername(Customer customer, string newUsername)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (!_customerSettings.UsernamesEnabled)
                throw new SmartException("Usernames are disabled");

            if (!_customerSettings.AllowUsersToChangeUsernames)
                throw new SmartException("Changing usernames is not allowed");

            newUsername = newUsername.Trim();

            if (newUsername.Length > 100)
                throw new SmartException(T("Account.EmailUsernameErrors.UsernameTooLong"));

            var user2 = _customerService.GetCustomerByUsername(newUsername);
            if (user2 != null && customer.Id != user2.Id)
                throw new SmartException(T("Account.EmailUsernameErrors.UsernameAlreadyExists"));

            customer.Username = newUsername;
            _customerService.UpdateCustomer(customer);
        }

        #endregion
    }
}