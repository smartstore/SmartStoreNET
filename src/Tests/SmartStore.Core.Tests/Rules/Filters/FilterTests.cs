using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartStore.Rules;
using SmartStore.Rules.Filters;

namespace SmartStore.Core.Tests.Rules.Filters
{
    [TestFixture]
    public class FilterTests : FilterTestsBase
    {
        [Test]
        public void OperatorIsNull()
        {
            var op = RuleOperator.IsNull;

            Assert.AreEqual(true, op.Match(null, null));
            Assert.AreEqual(false, op.Match("no", null));
            Assert.AreEqual(true, op.Match((int?)null, null));

            var expectedResult = Customers.Where(x => x.Username == null).ToList();
            var result = ExecuteQuery(op, x => x.Username, null);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorIsNotNull()
        {
            var op = RuleOperator.IsNotNull;

            Assert.AreEqual(false, op.Match(null, null));
            Assert.AreEqual(true, op.Match("no", null));
            Assert.AreEqual(false, op.Match((int?)null, null));

            var expectedResult = Customers.Where(x => x.Username != null).ToList();
            var result = ExecuteQuery(op, x => x.Username, null);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorIsEmpty()
        {
            var op = RuleOperator.IsEmpty;

            Assert.AreEqual(true, op.Match((string)null, null));
            Assert.AreEqual(true, op.Match(string.Empty, null));
            Assert.AreEqual(false, op.Match(" ab", null));

            var expectedResult = Customers.Where(x => string.IsNullOrWhiteSpace(x.Username)).ToList();
            var result = ExecuteQuery(op, x => x.Username, null);
            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorIsNotEmpty()
        {
            var op = RuleOperator.IsNotEmpty;

            Assert.AreEqual(false, op.Match((string)null, null));
            Assert.AreEqual(false, op.Match(string.Empty, null));
            Assert.AreEqual(true, op.Match(" ab", null));

            var expectedResult = Customers.Where(x => !string.IsNullOrEmpty(x.Username)).ToList();
            var result = ExecuteQuery(op, x => x.Username, null);
            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorEqualTo()
        {
            var op = RuleOperator.IsEqualTo;

            var d1 = (DateTime?)DateTime.Now.Date;
            var d2 = DateTime.Now.Date;
            var d3 = (DateTime?)null;
            var e1 = DateTimeKind.Utc;
            var e2 = (DateTimeKind?)DateTimeKind.Utc;
            var e3 = (DateTimeKind?)null;

            Assert.AreEqual(true, op.Match(null, null));
            Assert.AreEqual(true, op.Match(string.Empty, string.Empty));
            Assert.AreEqual(true, op.Match("abc", "abc"));
            Assert.AreEqual(true, op.Match(d1, d2));
            Assert.AreEqual(true, op.Match(d2, d1));
            Assert.AreEqual(false, op.Match(d3, d1));
            Assert.AreEqual(false, op.Match(d2, d3));
            Assert.AreEqual(true, op.Match(e1, e2));
            Assert.AreEqual(true, op.Match(e2, e1));
            Assert.AreEqual(false, op.Match(e3, e1));
            Assert.AreEqual(false, op.Match(e2, e3));

            var expectedResult = Customers.Where(x => x.IsTaxExempt == true).ToList();
            var result = ExecuteQuery(op, x => x.IsTaxExempt, true);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorNotEqualTo()
        {
            var op = RuleOperator.IsNotEqualTo;

            var d1 = (DateTime?)DateTime.Now.Date;
            var d2 = DateTime.Now.Date;
            var d3 = (DateTime?)null;
            var e1 = DateTimeKind.Utc;
            var e2 = (DateTimeKind?)DateTimeKind.Utc;
            var e3 = (DateTimeKind?)null;

            Assert.AreEqual(false, op.Match(null, null));
            Assert.AreEqual(false, op.Match(string.Empty, string.Empty));
            Assert.AreEqual(false, op.Match("abc", "abc"));
            Assert.AreEqual(false, op.Match(d1, d2));
            Assert.AreEqual(false, op.Match(d2, d1));
            Assert.AreEqual(true, op.Match(d3, d1));
            Assert.AreEqual(true, op.Match(d2, d3));
            Assert.AreEqual(false, op.Match(e1, e2));
            Assert.AreEqual(false, op.Match(e2, e1));
            Assert.AreEqual(true, op.Match(e3, e1));
            Assert.AreEqual(true, op.Match(e2, e3));

            var expectedResult = Customers.Where(x => x.IsTaxExempt == false).ToList();
            var result = ExecuteQuery(op, x => x.IsTaxExempt, true);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorStartsWith()
        {
            var op = RuleOperator.StartsWith;

            Assert.AreEqual(true, op.Match("hello", "he"));

            var expectedResult = Customers.Where(x => x.Username.EmptyNull().StartsWith("s")).ToList();
            var result = ExecuteQuery(op, x => x.Username, "s");

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorEndsWith()
        {
            var op = RuleOperator.EndsWith;

            Assert.AreEqual(true, op.Match("hello", "lo"));

            var expectedResult = Customers.Where(x => x.Username.EmptyNull().EndsWith("y")).ToList();
            var result = ExecuteQuery(op, x => x.Username, "y");

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorContains()
        {
            var op = RuleOperator.Contains;

            Assert.AreEqual(true, op.Match("hello", "el"));

            var expectedResult = Customers.Where(x => x.Username.EmptyNull().Contains("now")).ToList();
            var result = ExecuteQuery(op, x => x.Username, "now");

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorNotContains()
        {
            var op = RuleOperator.NotContains;

            Assert.AreEqual(true, op.Match("hello", "al"));

            var expectedResult = Customers.Where(x => !x.Username.EmptyNull().Contains("a")).ToList();
            var result = ExecuteQuery(op, x => x.Username, "a");

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorGreatherThan()
        {
            var op = RuleOperator.GreaterThan;

            Assert.AreEqual(true, op.Match(10, 5));
            Assert.AreEqual(false, op.Match(5, 10));

            var expectedResult = Customers.Where(x => x.BirthDate.HasValue && x.BirthDate > DateTime.Now.AddYears(-30)).ToList();
            var result = ExecuteQuery(op, x => x.BirthDate, DateTime.Now.AddYears(-30));

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorGreatherThanOrEqualTo()
        {
            var op = RuleOperator.GreaterThanOrEqualTo;

            Assert.AreEqual(true, op.Match(5, 5));
            Assert.AreEqual(false, op.Match(4, 5));

            var expectedResult = Customers.Where(x => x.Id >= 2).ToList();
            var result = ExecuteQuery(op, x => x.Id, 2);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorLessThan()
        {
            var op = RuleOperator.LessThan;

            Assert.AreEqual(true, op.Match(5, 10));
            Assert.AreEqual(false, op.Match(10, 5));

            var expectedResult = Customers.Where(x => x.BirthDate.HasValue && x.BirthDate < DateTime.Now.AddYears(-10)).ToList();
            var result = ExecuteQuery(op, x => x.BirthDate, DateTime.Now.AddYears(-10));

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorLessThanOrEqualTo()
        {
            var op = RuleOperator.LessThanOrEqualTo;

            Assert.AreEqual(true, op.Match(5, 5));
            Assert.AreEqual(false, op.Match(5, 4));

            var expectedResult = Customers.Where(x => x.Id <= 4).ToList();
            var result = ExecuteQuery(op, x => x.Id, 4);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorIn()
        {
            var op = RuleOperator.In;

            var orderIds = new List<int> { 1, 2, 5, 8 };
            Assert.AreEqual(true, op.Match(2, orderIds));
            Assert.AreEqual(false, op.Match(3, orderIds));

            var expectedResult = Customers.Where(x => orderIds.Contains(x.Id)).ToList();
            var result = ExecuteQuery(op, x => x.Id, orderIds);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorNotIn()
        {
            var op = RuleOperator.NotIn;

            var orderIds = new List<int> { 1, 2, 3, 5 };
            Assert.AreEqual(true, op.Match(4, orderIds));
            Assert.AreEqual(false, op.Match(2, orderIds));

            var expectedResult = Customers.Where(x => !orderIds.Contains(x.Id)).ToList();
            var result = ExecuteQuery(op, x => x.Id, orderIds);

            AssertEquality(expectedResult, result);
        }


        [Test]
        public void SimpleMemberFiltersMatchAnd()
        {
            var taxExemptFilter = new FilterExpression
            {
                Descriptor = FilterDescriptors.TaxExempt,
                Operator = RuleOperator.IsEqualTo,
                Value = true
            };

            var countryFilter = new FilterExpression
            {
                Descriptor = FilterDescriptors.BillingCountry,
                Operator = RuleOperator.IsEqualTo,
                Value = 2
            };

            var expectedResult = Customers
                .Where(x => x.IsTaxExempt && x.BillingAddress != null && x.BillingAddress.CountryId == 2)
                .ToList();
            var result = ExecuteQuery(LogicalRuleOperator.And, taxExemptFilter, countryFilter);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void SimpleMemberFiltersMatchOr()
        {
            var taxExemptFilter = new FilterExpression
            {
                Descriptor = FilterDescriptors.TaxExempt,
                Operator = RuleOperator.IsEqualTo,
                Value = true
            };

            var countryFilter = new FilterExpression
            {
                Descriptor = FilterDescriptors.BillingCountry,
                Operator = RuleOperator.IsEqualTo,
                Value = 2
            };

            var shippingCountryFilter = new FilterExpression
            {
                Descriptor = FilterDescriptors.ShippingCountry,
                Operator = RuleOperator.IsEqualTo,
                Value = 2
            };

            var expectedResult = Customers
                .Where(x => x.IsTaxExempt || x.BillingAddress?.CountryId == 2 || x.ShippingAddress?.CountryId == 2)
                .ToList();
            var result = ExecuteQuery(LogicalRuleOperator.Or, taxExemptFilter, countryFilter, shippingCountryFilter);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void CompletedOrderCount()
        {
            var filter = new FilterExpression
            {
                Descriptor = FilterDescriptors.CompletedOrderCount,
                Operator = RuleOperator.GreaterThanOrEqualTo,
                Value = 1
            };

            var expectedResult = Customers.Where(x => x.Orders.Count(y => y.OrderStatusId == 30) >= 1).ToList();
            var result = ExecuteQuery(LogicalRuleOperator.And, filter);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void HasAnyCustomerRole()
        {
            var filter = new FilterExpression
            {
                Descriptor = FilterDescriptors.IsInAnyRole,
                Operator = RuleOperator.In,
                Value = new List<int> { 2, 3 }
            };

            var roleIdsToCheck = filter.Value as List<int>;

            var expectedResult = Customers
                .Where(x => x.CustomerRoleMappings.Any(rm => rm.CustomerRole.Active && roleIdsToCheck.Contains(rm.CustomerRole.Id)))
                .ToList();

            var result = ExecuteQuery(LogicalRuleOperator.And, filter);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void HasAllCustomerRoles()
        {
            var filter = new FilterExpression
            {
                Descriptor = FilterDescriptors.HasAllRoles,
                Operator = RuleOperator.In,
                Value = new List<int> { 2, 3 }
            };

            var roleIdsToCheck = filter.Value as List<int>;

            var expectedResult = Customers
                .Where(x => x.CustomerRoleMappings.Where(rm => rm.CustomerRole.Active).All(rm => roleIdsToCheck.Contains(rm.CustomerRole.Id)))
                .ToList();

            var result = ExecuteQuery(LogicalRuleOperator.And, filter);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void HasPurchasedProduct()
        {
            var filter = new FilterExpression
            {
                Descriptor = FilterDescriptors.HasPurchasedProduct,
                Operator = RuleOperator.In,
                Value = new List<int> { 7, 8, 9, 10 }
            };

            var productIdsToCheck = filter.Value as List<int>;

            var expectedResult = Customers.Where(x => x.Orders.SelectMany(o => o.OrderItems).Any(p => productIdsToCheck.Contains(p.ProductId))).ToList();
            var result = ExecuteQuery(LogicalRuleOperator.And, filter);

            AssertEquality(expectedResult, result);
        }
    }
}
