using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Infrastructure;
using SmartStore.Rules;
using SmartStore.Rules.Filters;

namespace SmartStore.Core.Tests.Rules.Filters
{
    [TestFixture]
    public class FilterTests
    {
        private List<Customer> _customers;
        private List<FilterDescriptor> _descriptors;

        private CustomerRole _role1 = new CustomerRole { Id = 1, Active = true, TaxExempt = false, TaxDisplayType = 1 };
        private CustomerRole _role2 = new CustomerRole { Id = 2, Active = true, TaxExempt = true, TaxDisplayType = 1 };
        private CustomerRole _role3 = new CustomerRole { Id = 3, Active = false, TaxExempt = false, TaxDisplayType = 1 };
        private CustomerRole _role4 = new CustomerRole { Id = 4, Active = true, TaxExempt = false, TaxDisplayType = 2 };

        private Store _store1 = new Store { Id = 1 };
        private Store _store2 = new Store { Id = 2 };
        private Store _store3 = new Store { Id = 3 };

        private string _pay1 = "Payment1";
        private string _pay2 = "Payment2";
        private string _pay3 = "Payment3";

        private string _ship1 = "Ship1";
        private string _ship2 = "Ship2";
        private string _ship3 = "Ship3";

        private List<Product> _products = new List<Product>();

        [SetUp]
        public virtual void SetUp()
        {
            SetUpEntities();
            SetUpFilterDescriptors();
        }

        private void SetUpEntities()
        {

            for (int i = 1; i <= 10; i++)
            {
                _products.Add(new Product { Id = i });
            }

            _customers = new List<Customer>
            {
                new Customer
                {
                    Id = 1,
                    BillingAddress = new Address { CountryId = 1 }, ShippingAddress = new Address { CountryId = 1 },
                    BirthDate = new DateTime(1980, 1, 1),
                    CustomerRoles = new List<CustomerRole> { _role1, _role2 },
                    IsTaxExempt = false,
                    LastActivityDateUtc = DateTime.Now.AddDays(-5),
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            StoreId = _store1.Id,
                            OrderStatus = OrderStatus.Complete,
                            PaymentMethodSystemName = _pay1,
                            ShippingMethod = _ship1,
                            OrderTotal = 200,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem { ProductId = 1 }, new OrderItem { ProductId = 2 }, new OrderItem { ProductId = 3 }, new OrderItem { ProductId = 4 }
                            }
                        }
                    }
                },
                new Customer
                {
                    Id = 2,
                    BillingAddress = new Address { CountryId = 2 }, ShippingAddress = new Address { CountryId = 1 },
                    BirthDate = new DateTime(1999, 12, 24),
                    CustomerRoles = new List<CustomerRole> { _role2, _role3, _role4 },
                    IsTaxExempt = true,
                    LastActivityDateUtc = DateTime.Now.AddDays(-1),
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            StoreId = _store2.Id,
                            OrderStatus = OrderStatus.Processing,
                            PaymentMethodSystemName = _pay2,
                            ShippingMethod = _ship2,
                            OrderTotal = 999,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem { ProductId = 1 }, new OrderItem { ProductId = 2 }, new OrderItem { ProductId = 5 }, new OrderItem { ProductId = 6 }, new OrderItem { ProductId = 7 }, new OrderItem { ProductId = 8 }
                            }
                        }
                    }
                }
            };
        }

        private void SetUpFilterDescriptors()
        {
            _descriptors = new List<FilterDescriptor>
            {
                new FilterDescriptor<Customer, bool>(x => x.IsTaxExempt)
                {
                    Type = RuleType.Boolean,
                    Name = "TaxExempt"
                },
                new FilterDescriptor<Customer, int?>(x => x.BillingAddress.CountryId)
                {
                    Type = RuleType.NullableInt,
                    Name = "BillingCountry"
                },
                new FilterDescriptor<Customer, int?>(x => x.ShippingAddress.CountryId)
                {
                    Type = RuleType.NullableInt,
                    Name = "ShippingCountry"
                },
                new FilterDescriptor<Customer, double>(x => (DateTime.UtcNow - x.LastActivityDateUtc).TotalDays /* DbFunctions.DiffDays(x.LastActivityDateUtc, DateTime.UtcNow)*/)
                {
                    Type = RuleType.Int,
                    Name = "LastActivityDays"
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 30))
                {
                    Type = RuleType.Int,
                    Name = "CompletedOrderCount"
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 40))
                {
                    Type = RuleType.Int,
                    Name = "CancelledOrderCount"
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId >= 20))
                {
                    Type = RuleType.Int,
                    Name = "NewOrderCount"
                },
                new FilterDescriptor<Customer, double>(x => DateTime.Now.Year - x.BirthDate.Value.Year)
                {
                    Type = RuleType.Int,
                    Name = "Age"
                },
            };
        }

        [Test]
        public void IsTaxExemptAndCountryIs()
        {
            var taxExemptFilter = new FilterExpression
            {
                Descriptor = _descriptors.FirstOrDefault(x => x.Name == "TaxExempt"),
                Operator = RuleOperator.EqualTo,
                Value = true
            };

            var countryFilter = new FilterExpression
            {
                Descriptor = _descriptors.FirstOrDefault(x => x.Name == "BillingCountry"),
                Operator = RuleOperator.EqualTo,
                Value = 2
            };

            var compositeFilter = new CompositeFilterExpression(typeof(Customer));
            compositeFilter.AddExpressions(taxExemptFilter, countryFilter);

            var predicate = compositeFilter.GetFilterExpression();
            var result = _customers.AsQueryable().Where(predicate).Cast<Customer>().ToList();

            Assert.AreEqual(1, result.Count, "ResultCount");
            Assert.AreEqual(2, result.First().Id, "FirstId");
        }
    }
}
