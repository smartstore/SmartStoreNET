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
    public class FilterTests : FilterTestsBase
    {
        [Test]
        public void OperatorIsNull()
        {
            var op = RuleOperator.IsNull;

            var expectedResult = Customers.Where(x => x.Username == null).ToList();
            var result = ExecuteQuery(op, x => x.Username, null);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorIsNotNull()
        {
            var op = RuleOperator.IsNotNull;

            var expectedResult = Customers.Where(x => x.Username != null).ToList();
            var result = ExecuteQuery(op, x => x.Username, null);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorIsEmpty()
        {
            var op = RuleOperator.IsEmpty;

            var expectedResult = Customers.Where(x => string.IsNullOrEmpty(x.Username)).ToList();
            var result = ExecuteQuery(op, x => x.Username, null);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorIsNotEmpty()
        {
            var op = RuleOperator.IsNotEmpty;

            var expectedResult = Customers.Where(x => !string.IsNullOrEmpty(x.Username)).ToList();
            var result = ExecuteQuery(op, x => x.Username, null);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorEqualTo()
        {
            var op = RuleOperator.IsEqualTo;

            var expectedResult = Customers.Where(x => x.IsTaxExempt == true).ToList();
            var result = ExecuteQuery(op, x => x.IsTaxExempt, true);

            AssertEquality(expectedResult, result); 
        }

        [Test]
        public void OperatorNotEqualTo()
        {
            var op = RuleOperator.IsEqualTo;

            var expectedResult = Customers.Where(x => x.IsTaxExempt == false).ToList();
            var result = ExecuteQuery(op, x => x.IsTaxExempt, false);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorStartsWith()
        {
            var op = RuleOperator.StartsWith;

            var expectedResult = Customers.Where(x => x.Username.EmptyNull().StartsWith("s")).ToList();
            var result = ExecuteQuery(op, x => x.Username, "s");

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorEndsWith()
        {
            var op = RuleOperator.EndsWith;

            var expectedResult = Customers.Where(x => x.Username.EmptyNull().EndsWith("y")).ToList();
            var result = ExecuteQuery(op, x => x.Username, "y");

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorContains()
        {
            var op = RuleOperator.Contains;

            var expectedResult = Customers.Where(x => x.Username.EmptyNull().Contains("now")).ToList();
            var result = ExecuteQuery(op, x => x.Username, "now");

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorNotContains()
        {
            var op = RuleOperator.NotContains;

            var expectedResult = Customers.Where(x => !x.Username.EmptyNull().Contains("a")).ToList();
            var result = ExecuteQuery(op, x => x.Username, "a");

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorGreatherThan() 
        {
            var op = RuleOperator.GreaterThan;

            var expectedResult = Customers.Where(x => x.BirthDate.HasValue && x.BirthDate > DateTime.Now.AddYears(-30)).ToList();
            var result = ExecuteQuery(op, x => x.BirthDate, DateTime.Now.AddYears(-30));

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorGreatherThanOrEqualTo()
        {
            var op = RuleOperator.GreaterThanOrEqualTo;

            var expectedResult = Customers.Where(x => x.Id >= 2).ToList();
            var result = ExecuteQuery(op, x => x.Id, 2);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorLessThan()
        {
            var op = RuleOperator.LessThan;

            var expectedResult = Customers.Where(x => x.BirthDate.HasValue && x.BirthDate < DateTime.Now.AddYears(-10)).ToList();
            var result = ExecuteQuery(op, x => x.BirthDate, DateTime.Now.AddYears(-10));

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorLessThanOrEqualTo()
        {
            var op = RuleOperator.LessThanOrEqualTo;

            var expectedResult = Customers.Where(x => x.Id <= 4).ToList();
            var result = ExecuteQuery(op, x => x.Id, 4);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorIn()
        {
            var op = RuleOperator.In;

            var orderIds = new List<int> { 1, 2, 5, 8 };
            var expectedResult = Customers.Where(x => orderIds.Contains(x.Id)).ToList();
            var result = ExecuteQuery(op, x => x.Id, orderIds);

            AssertEquality(expectedResult, result);
        }

        [Test]
        public void OperatorNotIn()
        {
            var op = RuleOperator.NotIn;

            var orderIds = new List<int> { 1, 2, 3, 5 };
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
                Descriptor = FilterDescriptors.IsInRole,
                Operator = RuleOperator.In,
                Value = new List<int> { 2, 3 }
            };

            var roleIdsToCheck = filter.Value as List<int>;

            var expectedResult = Customers.Where(x => x.CustomerRoles.Any(r => r.Active && roleIdsToCheck.Contains(r.Id))).ToList();
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
