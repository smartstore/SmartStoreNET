using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Customer service interface
    /// </summary>
    public partial interface ICustomerService
    {
        #region Customers

        /// <summary>
        /// Finds customer records matching all criteria specified by <paramref name="q"/>
        /// </summary>
        /// <param name="q">The filter query</param>
        /// <returns>Customer collection</returns>
        IPagedList<Customer> SearchCustomers(CustomerSearchQuery q);

        /// <summary>
        /// Gets all customers by customer format (including deleted ones)
        /// </summary>
        /// <param name="passwordFormat">Password format</param>
        /// <returns>Customers</returns>
        IPagedList<Customer> GetAllCustomersByPasswordFormat(PasswordFormat passwordFormat);

        /// <summary>
        /// Gets online customers
        /// </summary>
        /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
        /// <param name="customerRoleIds">A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customer collection</returns>
        IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc, int[] customerRoleIds, int pageIndex, int pageSize);

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void DeleteCustomer(Customer customer);

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        Customer GetCustomerById(int customerId);

        /// <summary>
        /// Get customers by identifiers
        /// </summary>
        /// <param name="customerIds">Customer identifiers</param>
        /// <returns>Customers</returns>
        IList<Customer> GetCustomersByIds(int[] customerIds);

        /// <summary>
        /// Get system account customers
        /// </summary>
        /// <returns>System account customers</returns>
        IList<Customer> GetSystemAccountCustomers();

        /// <summary>
        /// Gets a customer by GUID
        /// </summary>
        /// <param name="customerGuid">Customer GUID</param>
        /// <returns>A customer</returns>
        Customer GetCustomerByGuid(Guid customerGuid);

        /// <summary>
        /// Get customer by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Customer</returns>
        Customer GetCustomerByEmail(string email);

        /// <summary>
        /// Get customer by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Customer</returns>
        Customer GetCustomerBySystemName(string systemName);

        /// <summary>
        /// Get customer by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Customer</returns>
        Customer GetCustomerByUsername(string username);

        /// <summary>
        /// Insert a guest customer
        /// </summary>
        /// <param name="customerGuid">The customer GUID. Pass <c>null</c> to create a random one.</param>
        /// <returns>Customer</returns>
        Customer InsertGuestCustomer(Guid? customerGuid = null);

        /// <summary>
        /// Tries to find a guest/anonymous customer record by client ident. This method should be called when an
        /// anonymous visitor rejects cookies and therefore cannot be identified automatically.
        /// </summary>
        /// <param name="clientIdent">
        /// The client ident string, which is a hashed combination of client IP address and user agent. 
        /// Call <see cref="IWebHelper.GetClientIdent()"/> to obtain an ident string, or pass <c>null</c> to let this method obtain it automatically.</param>
        /// <param name="maxAgeSeconds">The max age of the newly created guest customer record. The shorter, the better (default is 1 min.)</param>
        /// <returns>The identified customer or <c>null</c></returns>
        Customer FindGuestCustomerByClientIdent(string clientIdent = null, int maxAgeSeconds = 60);

        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void InsertCustomer(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomer(Customer customer);

        /// <summary>
        /// Reset data required for checkout
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="clearCouponCodes">A value indicating whether to clear coupon code</param>
        /// <param name="clearCheckoutAttributes">A value indicating whether to clear selected checkout attributes</param>
        /// <param name="clearRewardPoints">A value indicating whether to clear "Use reward points" flag</param>
        /// <param name="clearShippingMethod">A value indicating whether to clear selected shipping method</param>
        /// <param name="clearPaymentMethod">A value indicating whether to clear selected payment method</param>
        /// <param name="clearCreditBalance">A value indicating whether to clear credit balance.</param>
        void ResetCheckoutData(Customer customer, int storeId,
            bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = false, bool clearShippingMethod = true,
            bool clearPaymentMethod = true,
            bool clearCreditBalance = false);

        /// <summary>
        /// Delete guest customer records including generic attributes.
        /// </summary>
        /// <param name="registrationFrom">Customer registration from. <c>null</c> to ignore.</param>
        /// <param name="registrationTo">Customer registration to. <c>null</c> to ignore.</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete only customers without shopping cart.</param>
        /// <returns>Number of deleted guest customers.</returns>
        int DeleteGuestCustomers(
            DateTime? registrationFrom,
            DateTime? registrationTo,
            bool onlyWithoutShoppingCart);

        #endregion

        #region Customer roles

        /// <summary>
        /// Delete a customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        void DeleteCustomerRole(CustomerRole customerRole);

        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role identifier</param>
        /// <returns>Customer role</returns>
        CustomerRole GetCustomerRoleById(int customerRoleId);

        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="systemName">Customer role system name</param>
        /// <returns>Customer role</returns>
        CustomerRole GetCustomerRoleBySystemName(string systemName);

        /// <summary>
        /// Gets all customer roles
        /// </summary>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Customer role collection</returns>
        IPagedList<CustomerRole> GetAllCustomerRoles(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Inserts a customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        void InsertCustomerRole(CustomerRole customerRole);

        /// <summary>
        /// Updates the customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        void UpdateCustomerRole(CustomerRole customerRole);

        #endregion

        #region Customer role mappings

        /// <summary>
        /// Gets a customer role mapping by identifier.
        /// </summary>
        /// <param name="mappingId">Mapping identifier.</param>
        /// <returns>Customer role mapping.</returns>
        CustomerRoleMapping GetCustomerRoleMappingById(int mappingId);

        /// <summary>
        /// Gets customer role mappings.
        /// </summary>
        /// <param name="customerIds">Customer identifiers to be filtered by.</param>
        /// <param name="customerRoleIds">Customer role identifiers to be filtered by.</param>
        /// <param name="isSystemMapping">Whether to filter by system or user mappings.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="withCustomers">Whether to include customers through navigation property.</param>
        /// <returns>Customer role mappings.</returns>
        IPagedList<CustomerRoleMapping> GetCustomerRoleMappings(
            int[] customerIds,
            int[] customerRoleIds,
            bool? isSystemMapping,
            int pageIndex,
            int pageSize,
            bool withCustomers = true);

        /// <summary>
        /// Inserts a customer role mapping.
        /// </summary>
        /// <param name="mapping">Customer role mapping.</param>
        void InsertCustomerRoleMapping(CustomerRoleMapping mapping);

        /// <summary>
        /// Updates a customer role mapping.
        /// </summary>
        /// <param name="mapping">Customer role mapping.</param>
        void UpdateCustomerRoleMapping(CustomerRoleMapping mapping);

        /// <summary>
        /// Deletes a customer role mapping.
        /// </summary>
        /// <param name="mapping">Customer role mapping.</param>
        void DeleteCustomerRoleMapping(CustomerRoleMapping mapping);

        #endregion

        #region Reward points

        /// <summary>
        /// Add or remove reward points for a product review
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <param name="product">The product</param>
        /// <param name="add">Whether to add or remove points</param>
        void RewardPointsForProductReview(Customer customer, Product product, bool add);

        /// <summary>
        /// Gets reward points histories
        /// </summary>
        /// <param name="customerIds">Customer identifiers</param>
        /// <returns>Reward points histories</returns>
        Multimap<int, RewardPointsHistory> GetRewardPointsHistoriesByCustomerIds(int[] customerIds);

        #endregion
    }
}