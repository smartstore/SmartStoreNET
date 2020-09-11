using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Events;
using SmartStore.Core.Fakes;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Customers
{
    [TestFixture]
    public class CustomerRegistrationServiceTests : ServiceTest
    {
        IRepository<Customer> _customerRepo;
        IRepository<CustomerRole> _customerRoleRepo;
        IRepository<CustomerRoleMapping> _customerRoleMappingRepo;
        IRepository<GenericAttribute> _genericAttributeRepo;
        IRepository<RewardPointsHistory> _rewardPointsHistoryRepo;
        IRepository<ShoppingCartItem> _shoppingCartItemRepo;
        IGenericAttributeService _genericAttributeService;
        IEncryptionService _encryptionService;
        ICustomerService _customerService;
        ICustomerRegistrationService _customerRegistrationService;
        CustomerSettings _customerSettings;
        INewsLetterSubscriptionService _newsLetterSubscriptionService;
        IEventPublisher _eventPublisher;
        Lazy<RewardPointsSettings> _rewardPointsSettings;
        SecuritySettings _securitySettings;
        IStoreContext _storeContext;
        ICommonServices _services;
        IUserAgent _userAgent;
        Lazy<IGdprTool> _gdprTool;

        [SetUp]
        public new void SetUp()
        {
            _customerSettings = new CustomerSettings();
            _securitySettings = new SecuritySettings()
            {
                EncryptionKey = "273ece6f97dd844d"
            };
            _rewardPointsSettings = new Lazy<RewardPointsSettings>(() => new RewardPointsSettings
            {
                Enabled = false,
            });

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

            _customerRepo.Expect(x => x.Table).Return(new List<Customer> { customer1, customer2, customer3, customer4, customer5 }.AsQueryable());

            _customerRoleRepo = MockRepository.GenerateMock<IRepository<CustomerRole>>();
            _customerRoleMappingRepo = MockRepository.GenerateMock<IRepository<CustomerRoleMapping>>();
            _genericAttributeRepo = MockRepository.GenerateMock<IRepository<GenericAttribute>>();
            _rewardPointsHistoryRepo = MockRepository.GenerateMock<IRepository<RewardPointsHistory>>();
            _shoppingCartItemRepo = MockRepository.GenerateMock<IRepository<ShoppingCartItem>>();
            _userAgent = MockRepository.GenerateMock<IUserAgent>();

            _genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
            _newsLetterSubscriptionService = MockRepository.GenerateMock<INewsLetterSubscriptionService>();

            _storeContext = MockRepository.GenerateMock<IStoreContext>();

            _services = MockRepository.GenerateMock<ICommonServices>();
            _services.Expect(x => x.StoreContext).Return(_storeContext);
            _services.Expect(x => x.RequestCache).Return(NullRequestCache.Instance);
            _services.Expect(x => x.Cache).Return(NullCache.Instance);
            _services.Expect(x => x.EventPublisher).Return(_eventPublisher);

            _gdprTool = MockRepository.GenerateMock<Lazy<IGdprTool>>();

            _customerService = new CustomerService(
                _customerRepo,
                _customerRoleRepo,
                _customerRoleMappingRepo,
                _genericAttributeRepo,
                _rewardPointsHistoryRepo,
                _shoppingCartItemRepo,
                _genericAttributeService,
                _rewardPointsSettings,
                _services,
                new FakeHttpContext("~/"),
                _userAgent,
                new CustomerSettings(),
                _gdprTool);

            _customerRegistrationService = new CustomerRegistrationService(_customerService,
                _encryptionService, _newsLetterSubscriptionService, _rewardPointsSettings.Value, _customerSettings, _storeContext, _eventPublisher);
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
            customer.CustomerRoleMappings.Add(new CustomerRoleMapping
            {
                CustomerId = customer.Id,
                CustomerRole = new CustomerRole
                {
                    Active = true,
                    IsSystemRole = true,
                    SystemName = SystemCustomerRoleNames.Registered
                }
            });
        }
    }
}
