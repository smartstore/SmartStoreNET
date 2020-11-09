using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Rules;
using SmartStore.Rules.Filters;

namespace SmartStore.Core.Tests.Rules.Filters
{
    [TestFixture]
    public abstract class FilterTestsBase
    {
        private List<Customer> _customers;

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

        protected static class FilterDescriptors
        {
            public static FilterDescriptor TaxExempt = new FilterDescriptor<Customer, bool>(x => x.IsTaxExempt)
            {
                RuleType = RuleType.Boolean,
                Name = "TaxExempt"
            };
            public static FilterDescriptor BillingCountry = new FilterDescriptor<Customer, int?>(x => x.BillingAddress.CountryId)
            {
                RuleType = RuleType.NullableInt,
                Name = "BillingCountry"
            };
            public static FilterDescriptor ShippingCountry = new FilterDescriptor<Customer, int?>(x => x.ShippingAddress.CountryId)
            {
                RuleType = RuleType.NullableInt,
                Name = "ShippingCountry"
            };
            public static FilterDescriptor LastActivityDays = new FilterDescriptor<Customer, double>(x => (DateTime.UtcNow - x.LastActivityDateUtc).TotalDays /* DbFunctions.DiffDays(x.LastActivityDateUtc, DateTime.UtcNow)*/)
            {
                RuleType = RuleType.Int,
                Name = "LastActivityDays"
            };
            public static FilterDescriptor CompletedOrderCount = new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 30))
            {
                RuleType = RuleType.Int,
                Name = "CompletedOrderCount"
            };
            public static FilterDescriptor CancelledOrderCount = new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 40))
            {
                RuleType = RuleType.Int,
                Name = "CancelledOrderCount"
            };
            public static FilterDescriptor NewOrderCount = new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId >= 20))
            {
                RuleType = RuleType.Int,
                Name = "NewOrderCount"
            };
            public static FilterDescriptor Age = new FilterDescriptor<Customer, double>(x => DateTime.Now.Year - x.BirthDate.Value.Year)
            {
                RuleType = RuleType.Int,
                Name = "Age"
            };
            public static FilterDescriptor IsInAnyRole = new AnyFilterDescriptor<Customer, CustomerRoleMapping, int>(x => x.CustomerRoleMappings.Where(y => y.CustomerRole.Active), r => r.CustomerRole.Id)
            {
                RuleType = RuleType.IntArray,
                Name = "IsInAnyRole"
            };
            public static FilterDescriptor HasAllRoles = new AllFilterDescriptor<Customer, CustomerRoleMapping, int>(x => x.CustomerRoleMappings.Where(y => y.CustomerRole.Active), r => r.CustomerRole.Id)
            {
                RuleType = RuleType.IntArray,
                Name = "HasAllRoles"
            };
            public static FilterDescriptor HasPurchasedProduct = new AnyFilterDescriptor<Customer, OrderItem, int>(x => x.Orders.SelectMany(o => o.OrderItems), oi => oi.ProductId)
            {
                RuleType = RuleType.IntArray,
                Name = "HasPurchasedProduct"
            };
            // TODO
            public static FilterDescriptor AverageOrderItemPrice = new FilterDescriptor<Customer, decimal>(x => x.Orders.SelectMany(o => o.OrderItems.Where(oi => true)).Average(y => y.PriceInclTax))
            {
                RuleType = RuleType.Money,
                Name = "AverageOrderItemPrice"
            };
            public static FilterDescriptor TotalPurchasedUnits = new FilterDescriptor<Customer, int>(x => x.Orders.SelectMany(o => o.OrderItems.Where(oi => true)).Sum(y => y.Quantity))
            {
                RuleType = RuleType.Int,
                Name = "TotalPurchasedUnits"
            };
        }

        protected List<Customer> Customers => _customers;

        protected void AssertEquality(List<Customer> expected, List<Customer> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, "ResultCount");
            Console.WriteLine("Customer filter result - expected: {0}".FormatInvariant(expected.Count));

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Id, actual[i].Id, "Item " + i);
            }
        }

        [SetUp]
        public virtual void SetUp()
        {
            SetUpEntities();
        }

        protected virtual void SetUpEntities()
        {
            for (int i = 1; i <= 10; i++)
            {
                _products.Add(new Product { Id = i });
            }

            _customers = new List<Customer>();

            var customer1 = new Customer
            {
                Id = 1,
                Username = "sally",
                BillingAddress = new Address { CountryId = 1 },
                ShippingAddress = new Address { CountryId = 1 },
                BirthDate = new DateTime(1980, 1, 1),
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
                                new OrderItem { ProductId = 1, Quantity = 1, PriceInclTax = 10 },
                                new OrderItem { ProductId = 2, Quantity = 1, PriceInclTax = 50 },
                                new OrderItem { ProductId = 3, Quantity = 2, PriceInclTax = 100 },
                                new OrderItem { ProductId = 4, Quantity = 1, PriceInclTax = 40 }
                            }
                        }
                    }
            };
            customer1.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 1, CustomerRoleId = _role1.Id, CustomerRole = _role1 });
            customer1.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 1, CustomerRoleId = _role2.Id, CustomerRole = _role2 });
            _customers.Add(customer1);

            var customer2 = new Customer
            {
                Id = 2,
                Username = "john",
                BillingAddress = new Address { CountryId = 2 },
                ShippingAddress = new Address { CountryId = 1 },
                BirthDate = new DateTime(1999, 12, 24),
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
                                new OrderItem { ProductId = 1, Quantity = 3, PriceInclTax = 100 },
                                new OrderItem { ProductId = 2, Quantity = 1, PriceInclTax = 50 },
                                new OrderItem { ProductId = 5, Quantity = 1, PriceInclTax = 150 },
                                new OrderItem { ProductId = 6, Quantity = 1, PriceInclTax = 10 },
                                new OrderItem { ProductId = 7, Quantity = 2, PriceInclTax = 300 },
                                new OrderItem { ProductId = 8, Quantity = 4, PriceInclTax = 300 }
                            }
                        }
                    }
            };
            customer2.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 2, CustomerRoleId = _role2.Id, CustomerRole = _role2 });
            customer2.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 2, CustomerRoleId = _role3.Id, CustomerRole = _role3 });
            customer2.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 2, CustomerRoleId = _role4.Id, CustomerRole = _role4 });
            _customers.Add(customer2);

            var customer3 = new Customer
            {
                Id = 3,
                Username = "edward",
                BillingAddress = new Address { CountryId = 3 },
                ShippingAddress = new Address { CountryId = 3 },
                BirthDate = new DateTime(1960, 10, 10),
                IsTaxExempt = false,
                LastActivityDateUtc = DateTime.Now.AddDays(-100),
                Orders = new List<Order>
                    {
                        new Order
                        {
                            StoreId = _store1.Id,
                            OrderStatus = OrderStatus.Cancelled,
                            PaymentMethodSystemName = _pay3,
                            ShippingMethod = _ship3,
                            OrderTotal = 50,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem { ProductId = 2, Quantity = 1, PriceInclTax = 10 },
                                new OrderItem { ProductId = 3, Quantity = 1, PriceInclTax = 20 },
                                new OrderItem { ProductId = 5, Quantity = 1, PriceInclTax = 5 },
                                new OrderItem { ProductId = 8, Quantity = 1, PriceInclTax = 5 },
                                new OrderItem { ProductId = 9, Quantity = 1, PriceInclTax = 10 }
                            }
                        }
                    }
            };
            customer3.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 3, CustomerRoleId = _role1.Id, CustomerRole = _role1 });
            customer3.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 3, CustomerRoleId = _role4.Id, CustomerRole = _role4 });
            _customers.Add(customer3);

            var customer4 = new Customer
            {
                Id = 4,
                Username = "snowden",
                BillingAddress = new Address { CountryId = 1 },
                ShippingAddress = new Address { CountryId = 1 },
                BirthDate = DateTime.Now.AddYears(-32),
                IsTaxExempt = true,
                LastActivityDateUtc = DateTime.Now.AddDays(0),
                Orders = new List<Order>
                    {
                        new Order
                        {
                            StoreId = _store2.Id,
                            OrderStatus = OrderStatus.Complete,
                            PaymentMethodSystemName = _pay2,
                            ShippingMethod = _ship1,
                            OrderTotal = 500,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem { ProductId = 1, Quantity = 1, PriceInclTax = 250 },
                                new OrderItem { ProductId = 5, Quantity = 1, PriceInclTax = 250 }
                            }
                        }
                    }
            };
            customer4.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 4, CustomerRoleId = _role2.Id, CustomerRole = _role2 });
            customer4.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 4, CustomerRoleId = _role3.Id, CustomerRole = _role3 });
            _customers.Add(customer4);

            var customer5 = new Customer
            {
                Id = 5,
                BillingAddress = new Address { CountryId = 2 },
                ShippingAddress = new Address { CountryId = 2 },
                LastActivityDateUtc = DateTime.Now.AddDays(-2),
                Orders = new List<Order>
                    {
                        new Order
                        {
                            OrderItems = new List<OrderItem>()
                        }
                    }
            };
            customer5.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 5, CustomerRoleId = _role1.Id, CustomerRole = _role1 });
            _customers.Add(customer5);

            var customer6 = new Customer
            {
                Id = 6,
                BillingAddress = new Address { CountryId = 1 },
                ShippingAddress = new Address { CountryId = 1 },
                LastActivityDateUtc = DateTime.Now.AddDays(-10),
                Orders = new List<Order>
                    {
                        new Order
                        {
                            OrderItems = new List<OrderItem>()
                        }
                    }
            };
            customer6.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = 6, CustomerRoleId = _role2.Id, CustomerRole = _role2 });
            _customers.Add(customer6);
        }

        protected virtual List<Customer> ExecuteQuery<TValue>(RuleOperator op, Expression<Func<Customer, TValue>> left, object right)
        {
            var paramExpr = Expression.Parameter(typeof(Customer), "it");
            var valueExpr = ExpressionHelper.CreateValueExpression(left.Body.Type, right); // Expression.Constant(right)
            var expr = op.GetExpression(left.Body, valueExpr, true);
            var predicate = ExpressionHelper.CreateLambdaExpression(paramExpr, expr);

            var result = _customers.AsQueryable().Where(predicate).Cast<Customer>().ToList();

            return result;
        }

        protected virtual List<Customer> ExecuteQuery(LogicalRuleOperator logicalOperator, params FilterExpression[] expressions)
        {
            var compositeFilter = new FilterExpressionGroup(typeof(Customer)) { LogicalOperator = logicalOperator };
            compositeFilter.AddExpressions(expressions);

            var predicate = compositeFilter.ToPredicate(true);
            var result = _customers.AsQueryable().Where(predicate).Cast<Customer>().ToList();

            return result;
        }
    }
}
