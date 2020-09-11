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
    public partial class CustomerRegistrationService : ICustomerRegistrationService
    {
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;

        public CustomerRegistrationService(ICustomerService customerService,
            IEncryptionService encryptionService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            RewardPointsSettings rewardPointsSettings, CustomerSettings customerSettings,
            IStoreContext storeContext, IEventPublisher eventPublisher)
        {
            _customerService = customerService;
            _encryptionService = encryptionService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _rewardPointsSettings = rewardPointsSettings;
            _customerSettings = customerSettings;
            _storeContext = storeContext;
            _eventPublisher = eventPublisher;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public virtual bool ValidateCustomer(string usernameOrEmail, string password)
        {
            Customer customer = null;

            if (_customerSettings.CustomerLoginType == CustomerLoginType.Email)
            {
                customer = _customerService.GetCustomerByEmail(usernameOrEmail);
            }
            else if (_customerSettings.CustomerLoginType == CustomerLoginType.Username)
            {
                customer = _customerService.GetCustomerByUsername(usernameOrEmail);
            }
            else
            {
                customer = _customerService.GetCustomerByEmail(usernameOrEmail) ?? _customerService.GetCustomerByUsername(usernameOrEmail);
            }

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

        public virtual CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request)
        {
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(request.Customer, nameof(request.Customer));

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

            if (!request.Email.HasValue())
            {
                result.AddError(T("Account.Register.Errors.EmailIsNotProvided"));
                return result;
            }

            if (!request.Email.IsEmail())
            {
                result.AddError(T("Common.WrongEmail"));
                return result;
            }

            if (!request.Password.HasValue())
            {
                result.AddError(T("Account.Register.Errors.PasswordIsNotProvided"));
                return result;
            }

            if (_customerSettings.CustomerLoginType != CustomerLoginType.Email && !request.Username.HasValue())
            {
                result.AddError(T("Account.Register.Errors.UsernameIsNotProvided"));
                return result;
            }

            // Validate unique user
            if (_customerService.GetCustomerByEmail(request.Email) != null)
            {
                result.AddError(T("Account.Register.Errors.EmailAlreadyExists"));
                return result;
            }

            if (_customerSettings.CustomerLoginType != CustomerLoginType.Email && _customerService.GetCustomerByUsername(request.Username) != null)
            {
                result.AddError(T("Account.Register.Errors.UsernameAlreadyExists"));
                return result;
            }

            // At this point request is valid
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

            var registeredRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredRole == null)
            {
                throw new SmartException(T("Admin.Customers.CustomerRoles.CannotFoundRole", "Registered"));
            }

            if (_customerSettings.RegisterCustomerRoleId != 0)
            {
                var customerRole = _customerService.GetCustomerRoleById(_customerSettings.RegisterCustomerRoleId);
                if (customerRole != null && customerRole.Id != registeredRole.Id)
                {
                    _customerService.InsertCustomerRoleMapping(new CustomerRoleMapping { CustomerId = request.Customer.Id, CustomerRoleId = customerRole.Id });
                }
            }

            // Add to 'Registered' role.
            _customerService.InsertCustomerRoleMapping(new CustomerRoleMapping { CustomerId = request.Customer.Id, CustomerRoleId = registeredRole.Id });

            // Remove from 'Guests' role.
            var mappings = request.Customer.CustomerRoleMappings.Where(x => !x.IsSystemMapping && x.CustomerRole.SystemName == SystemCustomerRoleNames.Guests).ToList();
            mappings.Each(x => _customerService.DeleteCustomerRoleMapping(x));

            // Add reward points for customer registration (if enabled)
            if (_rewardPointsSettings.Enabled && _rewardPointsSettings.PointsForRegistration > 0)
            {
                request.Customer.AddRewardPointsHistoryEntry(_rewardPointsSettings.PointsForRegistration, T("RewardPoints.Message.RegisteredAsCustomer"));
            }

            _customerService.UpdateCustomer(request.Customer);
            _eventPublisher.Publish(new CustomerRegisteredEvent { Customer = request.Customer });

            return result;
        }

        public virtual PasswordChangeResult ChangePassword(ChangePasswordRequest request)
        {
            Guard.NotNull(request, nameof(request));

            var result = new PasswordChangeResult();
            if (!request.Email.HasValue())
            {
                result.AddError(T("Account.ChangePassword.Errors.EmailIsNotProvided"));
                return result;
            }
            if (!request.NewPassword.HasValue())
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

        public virtual void SetEmail(Customer customer, string newEmail)
        {
            Guard.NotNull(customer, nameof(customer));

            newEmail = newEmail.Trim();
            string oldEmail = customer.Email;

            if (!newEmail.IsEmail())
                throw new SmartException(T("Account.EmailUsernameErrors.NewEmailIsNotValid"));

            if (newEmail.Length > 100)
                throw new SmartException(T("Account.EmailUsernameErrors.EmailTooLong"));

            var newCustomer = _customerService.GetCustomerByEmail(newEmail);
            if (newCustomer != null && customer.Id != newCustomer.Id)
                throw new SmartException(T("Account.EmailUsernameErrors.EmailAlreadyExists"));

            customer.Email = newEmail;
            _customerService.UpdateCustomer(customer);

            //update newsletter subscription (if required)
            if (oldEmail.HasValue() && !oldEmail.Equals(newEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                var subscriptionOld = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(oldEmail, _storeContext.CurrentStore.Id);
                if (subscriptionOld != null)
                {
                    subscriptionOld.Email = newEmail;
                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscriptionOld);
                }
            }
        }

        public virtual void SetUsername(Customer customer, string newUsername)
        {
            Guard.NotNull(customer, nameof(customer));

            if (_customerSettings.CustomerLoginType == CustomerLoginType.Email)
                throw new SmartException("Usernames are disabled");

            if (!_customerSettings.AllowUsersToChangeUsernames)
                throw new SmartException("Changing usernames is not allowed");

            newUsername = newUsername.Trim();

            if (newUsername.Length > 100)
                throw new SmartException(T("Account.EmailUsernameErrors.UsernameTooLong"));

            var newUser = _customerService.GetCustomerByUsername(newUsername);
            if (newUser != null && customer.Id != newUser.Id)
                throw new SmartException(T("Account.EmailUsernameErrors.UsernameAlreadyExists"));

            customer.Username = newUsername;
            _customerService.UpdateCustomer(customer);
        }
    }
}