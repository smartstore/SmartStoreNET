using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Security;
using SmartStore.Core.Caching;
using Rhino.Mocks;
using SmartStore.Core.Domain.Common;

namespace SmartStore.Services.Tests.Customers
{
    [TestFixture]
    class CustomerServiceTest : ServiceTest
    {
        private IEncryptionService _encryptionService;
        private SecuritySettings _securitySettings;
        private ICustomerService _customerService;
        private IRepository<Customer> _customerRepo;
        IRepository<GenericAttribute> _genericAttributeRepo;

        [SetUp]
        public new void SetUp()
        {
            _securitySettings = new SecuritySettings();
            _encryptionService = new EncryptionService(_securitySettings);
            _customerRepo = MockRepository.GenerateMock<IRepository<Customer>>();
            _genericAttributeRepo = MockRepository.GenerateMock<IRepository<GenericAttribute>>();
            var customer1 = new Customer
            {
                Username = "a@b.com",
                Email = "a@b.com",
                PasswordFormat = PasswordFormat.Hashed,
                Active = true,
                Id = 1
            };
            var att1 = new GenericAttribute
            {
                EntityId = 1,
                Key = SystemCustomerAttributeNames.FirstName,
                Value = "John",
                KeyGroup = "Customer"
            };
            var att2 = new GenericAttribute
            {
                EntityId = 1,
                Key = SystemCustomerAttributeNames.LastName,
                Value = "Abot",
                KeyGroup = "Customer"
            };

            var customer2 = new Customer
            {
                Username = "test@test.com",
                Email = "test@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true,
                Id = 2
            };

            var customer3 = new Customer
            {
                Username = "user@test.com",
                Email = "user@test.com",
                PasswordFormat = PasswordFormat.Encrypted,
                Password = _encryptionService.EncryptText("password"),
                Active = true,
                Id = 3
            };

            var customer4 = new Customer
            {
                Username = "registered@test.com",
                Email = "registered@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true,
                Id = 4
            };

            var customer5 = new Customer
            {
                Username = "notregistered@test.com",
                Email = "notregistered@test.com",
                PasswordFormat = PasswordFormat.Clear,
                Password = "password",
                Active = true,
                Id = 5
            };

            _genericAttributeRepo.Expect(x => x.Table).Return(new List<GenericAttribute> { att1, att2 }.AsQueryable());
            _customerRepo.Expect(x => x.Table).Return(new List<Customer> { customer1, customer2, customer3, customer4, customer5 }.AsQueryable());
            _customerService = new CustomerService(new NullCache(), _customerRepo, null,
            _genericAttributeRepo, null, null, null);
        }
        
        /// <summary>
        /// The test verifies if the CustomerInformation class is working properly when querying a list of customer (long parameter list)
        /// It also verifies if the long method still works properly when querying by different attributes. 
        /// </summary>
        [Test]
        public void TestGetCustomerWithQuery()
        {
            TestCustomerEmail();

            TestAllCustomersByPage(0);

            TestAllCustomersByPage(2);

            TestCustomerFirstAndLastName("John", "Abot");
        }

        private void TestCustomerFirstAndLastName(string firstName, string lastName)
        {
            var infoAllCustomers = new CustomerInformation.Builder().SetPageSize(2).SetPageIndex(0).SetFirstName(firstName).SetLastName(lastName).Build();
            var pages = _customerService.GetAllCustomers(infoAllCustomers);
            Assert.AreEqual(1, pages.Count);
            Assert.AreEqual(1, pages.First().Id);
        }

        private void TestAllCustomersByPage(int pageIndex)
        {
            var infoAllCustomers = new CustomerInformation.Builder().SetPageSize(2).SetPageIndex(pageIndex).Build();
            var pages = _customerService.GetAllCustomers(infoAllCustomers);
            Assert.AreEqual(3, pages.TotalPages);
            //Assert.AreEqual(2, pages.Count);
            //Assert.AreEqual(pageIndex < pages.TotalPages - 1, pages.HasNextPage);
            if (pageIndex == pages.TotalPages - 1)
            {
                Assert.AreEqual(1, pages.Count);
                Assert.IsFalse(pages.HasNextPage);
            }
            else
            {
                Assert.AreEqual(2, pages.Count);
                Assert.IsTrue(pages.HasNextPage);
            }

            /*if (pageIndex == pages.TotalPages - 1)
                Assert.IsFalse(pages.HasNextPage);
            else
                Assert.IsTrue(pages.HasNextPage);*/
        }

        private void TestCustomerEmail()
        {
            var infoByEmail = new CustomerInformation.Builder().SetEmail("notregistered@test.com").SetPageSize(5).Build();
            var pagedCustomers = _customerService.GetAllCustomers(infoByEmail);
            Assert.AreEqual(1, pagedCustomers.Count);
        }
    }
}
