﻿using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Events;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Tests;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;

namespace SmartStore.Services.Tests.Customers
{
    [TestFixture]
    public class CustomerRegistrationServiceTests : ServiceTest
    {
        IRepository<Customer> _customerRepo;
        IRepository<CustomerRole> _customerRoleRepo;
        IRepository<GenericAttribute> _genericAttributeRepo;
        IGenericAttributeService _genericAttributeService;
        IEncryptionService _encryptionService;
        ICustomerService _customerService;
        ICustomerRegistrationService _customerRegistrationService;
        ILocalizationService _localizationService;
        CustomerSettings _customerSettings;
        INewsLetterSubscriptionService _newsLetterSubscriptionService;
        IEventPublisher _eventPublisher;
        RewardPointsSettings _rewardPointsSettings;
        SecuritySettings _securitySettings;
		IStoreContext _storeContext;

        [SetUp]
        public new void SetUp()
        {
            _customerSettings = new CustomerSettings();
            _securitySettings = new SecuritySettings()
            {
                EncryptionKey = "273ece6f97dd844d"
            };
            _rewardPointsSettings = new RewardPointsSettings()
            {
                Enabled = false,
            };

            _encryptionService = new EncryptionService(_securitySettings);
            _customerRepo = MockRepository.GenerateMock<IRepository<Customer>>();
            var customer1 = new Customer()
            {
                Username = "a@b.com",
                Email = "a@b.com",
                PasswordFormat = PasswordFormat.Hashed,
                Active = true
            };

            string saltKey = _encryptionService.CreateSaltKey(5);
            string password = _encryptionService.CreatePasswordHash("password", saltKey);
            customer1.PasswordSalt = saltKey;
            customer1.Password = password;
            AddCustomerToRegisteredRole(customer1);

            var customer2 = new Customer()
            {
                Username = "test@test.com",
                Email = "test@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };
            AddCustomerToRegisteredRole(customer2);

            var customer3 = new Customer()
            {
                Username = "user@test.com",
                Email = "user@test.com",
                PasswordFormat = PasswordFormat.Encrypted,
                Password = _encryptionService.EncryptText("password"),
                Active = true
            };
            AddCustomerToRegisteredRole(customer3);

            var customer4 = new Customer()
            {
                Username = "registered@test.com",
                Email = "registered@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };
            AddCustomerToRegisteredRole(customer4);

            var customer5 = new Customer()
            {
                Username = "notregistered@test.com",
                Email = "notregistered@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true
            };

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

            _customerRepo.Expect(x => x.Table).Return(new List<Customer>() { customer1, customer2, customer3, customer4, customer5 }.AsQueryable());

            _customerRoleRepo = MockRepository.GenerateMock<IRepository<CustomerRole>>();
            _genericAttributeRepo = MockRepository.GenerateMock<IRepository<GenericAttribute>>();

            _genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
            _newsLetterSubscriptionService = MockRepository.GenerateMock<INewsLetterSubscriptionService>();
            
            _localizationService = MockRepository.GenerateMock<ILocalizationService>();
			_storeContext = MockRepository.GenerateMock<IStoreContext>();

            _customerService = new CustomerService(new NullCache(), _customerRepo, _customerRoleRepo,
                _genericAttributeRepo, _genericAttributeService, _eventPublisher, _rewardPointsSettings);

            _customerRegistrationService = new CustomerRegistrationService(_customerService,
                _encryptionService, _newsLetterSubscriptionService, _localizationService,
                _rewardPointsSettings, _customerSettings, _storeContext, _eventPublisher);
        }

        //[Test]
        //public void Can_register_a_customer() 
        //{
        //    var registrationRequest = CreateCustomerRegistrationRequest();
        //    var result = _customerService.RegisterCustomer(registrationRequest);

        //    result.Success.ShouldBeTrue();
        //}

        //[Test]
        //public void Can_not_have_duplicate_usernames_or_emails() 
        //{
        //    var registrationRequest = CreateUserRegistrationRequest();
        //    registrationRequest.Username = "a@b.com";
        //    registrationRequest.Email = "a@b.com";

        //    var userService = new UserService(_encryptionService, _userRepo, _userSettings);
        //    var result = userService.RegisterUser(registrationRequest);

        //    result.Success.ShouldBeFalse();
        //    result.Errors.Count.ShouldEqual(1);
        //}

        [Test]
        public void Ensure_only_registered_customers_can_login()
        {
            bool result = _customerRegistrationService.ValidateCustomer("registered@test.com", "password");
            result.ShouldBeTrue();

            result = _customerRegistrationService.ValidateCustomer("notregistered@test.com", "password");
            result.ShouldBeFalse();
        }

        [Test]
        public void Can_validate_a_hashed_password() 
        {
            bool result = _customerRegistrationService.ValidateCustomer("a@b.com", "password");
            result.ShouldBeTrue();
        }

        [Test]
        public void Can_validate_a_clear_password() 
        {
            bool result = _customerRegistrationService.ValidateCustomer("test@test.com", "password");
            result.ShouldBeTrue();
        }

        [Test]
        public void Can_validate_an_encrypted_password() 
        {
            bool result = _customerRegistrationService.ValidateCustomer("user@test.com", "password");
            result.ShouldBeTrue();
        }

        //private CustomerRegistrationRequest CreateCustomerRegistrationRequest(Customer customer) 
        //{
        //    return new CustomerRegistrationRequest(customer, "test.user@domain.com", "test.user@domain.com", 
        //        "password", PasswordFormat.Encrypted);
        //}

        private void AddCustomerToRegisteredRole(Customer customer)
        {
            customer.CustomerRoles.Add(new CustomerRole()
            {
                Active =  true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered
            });
        }
    }
}
