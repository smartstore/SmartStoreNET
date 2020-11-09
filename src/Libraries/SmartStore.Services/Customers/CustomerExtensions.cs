using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Utilities;

namespace SmartStore.Services.Customers
{
    public static class CustomerExtentions
    {
        private static readonly string[] _systemColors = new string[] { "primary", "secondary", "success", "info", "warning", "danger", "light", "dark" };

        /// <summary>
        /// Gets a value indicating whether customer is in a certain customer role
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="customerRoleSystemName">Customer role system name</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        public static bool IsInCustomerRole(this Customer customer, string customerRoleSystemName, bool onlyActiveCustomerRoles = true)
        {
            Guard.NotNull(customer, nameof(customer));
            Guard.NotEmpty(customerRoleSystemName, nameof(customerRoleSystemName));

            var result = customer.CustomerRoleMappings
                .Where(rm => !onlyActiveCustomerRoles || rm.CustomerRole.Active)
                .Where(rm => rm.CustomerRole.SystemName == customerRoleSystemName)
                .FirstOrDefault() != null;

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the customer is a built-in record for background tasks
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsBackgroundTaskAccount(this Customer customer)
        {
            Guard.NotNull(customer, nameof(customer));

            if (!customer.IsSystemAccount || customer.SystemName.IsEmpty())
                return false;

            var result = customer.SystemName.Equals(SystemCustomerNames.BackgroundTask, StringComparison.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customer is a search engine
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsSearchEngineAccount(this Customer customer)
        {
            Guard.NotNull(customer, nameof(customer));

            if (!customer.IsSystemAccount || customer.SystemName.IsEmpty())
                return false;

            var result = customer.SystemName.Equals(SystemCustomerNames.SearchEngine, StringComparison.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customer is the pdf converter
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsPdfConverter(this Customer customer)
        {
            Guard.NotNull(customer, nameof(customer));

            if (!customer.IsSystemAccount || customer.SystemName.IsEmpty())
                return false;

            var result = customer.SystemName.Equals(SystemCustomerNames.PdfConverter, StringComparison.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customer is administrator
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAdmin(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Administrators, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Gets a value indicating whether customer is super administrator
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSuperAdmin(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.SuperAdministrators, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Gets a value indicating whether customer is a forum moderator
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsForumModerator(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.ForumModerators, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Gets a value indicating whether customer is registered
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRegistered(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Registered, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Gets a value indicating whether customer is guest
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGuest(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Guests, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Gets the customer's full name.
        /// </summary>
        /// <param name="customer">Customer.</param>
        /// <returns>Customer full name.</returns>
        public static string GetFullName(this Customer customer)
        {
            if (customer == null)
                return string.Empty;

            if (customer.FullName.HasValue())
            {
                return customer.FullName;
            }

            string name = customer.BillingAddress?.GetFullName();
            if (name.IsEmpty())
            {
                name = customer.ShippingAddress?.GetFullName();
            }
            if (name.IsEmpty())
            {
                name = customer.Addresses.FirstOrDefault()?.GetFullName();
            }

            return name.TrimSafe();
        }

        /// <summary>
        /// Gets the display name of a customer (full name, user name or email).
        /// </summary>
        /// <param name="customer">Customer.</param>
        /// <param name="T">Localizer.</param>
        /// <returns>Display name of a customer.</returns>
        public static string GetDisplayName(this Customer customer, Localizer T)
        {
            if (customer != null)
            {
                return customer.IsGuest()
                    ? T("Customer.Guest").Text
                    : customer.GetFullName().NullEmpty() ?? customer.Username ?? customer.FindEmail();
            }

            return null;
        }

        /// <summary>
        /// Formats the customer name.
        /// </summary>
        /// <param name="customer">Customer entity.</param>
        /// <returns>Formatted customer name.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatUserName(this Customer customer)
        {
            return FormatUserName(customer, false);
        }

        /// <summary>
        /// Formats the customer name.
        /// </summary>
        /// <param name="customer">Customer entity.</param>
        /// <param name="stripTooLong">Whether to strip too long customer name.</param>
        /// <returns>Formatted customer name.</returns>
        public static string FormatUserName(this Customer customer, bool stripTooLong)
        {
            var engine = EngineContext.Current;

            var userName = customer.FormatUserName(
                engine.Resolve<CustomerSettings>(),
                engine.Resolve<Localizer>(),
                stripTooLong);

            return userName;
        }

        /// <summary>
        /// Formats the customer name.
        /// </summary>
        /// <param name="customer">Customer entity.</param>
        /// <param name="customerSettings">Customer settings.</param>
        /// <param name="T">Localizer.</param>
        /// <param name="stripTooLong">Whether to strip too long customer name.</param>
        /// <returns>Formatted customer name.</returns>
        public static string FormatUserName(
            this Customer customer,
            CustomerSettings customerSettings,
            Localizer T,
            bool stripTooLong)
        {
            Guard.NotNull(customerSettings, nameof(customerSettings));
            Guard.NotNull(T, nameof(T));

            if (customer == null)
            {
                return string.Empty;
            }
            if (customer.IsGuest())
            {
                return T("Customer.Guest");
            }

            var result = string.Empty;

            switch (customerSettings.CustomerNameFormat)
            {
                case CustomerNameFormat.ShowEmails:
                    result = customer.Email;
                    break;
                case CustomerNameFormat.ShowFullNames:
                    result = customer.GetFullName();
                    break;
                case CustomerNameFormat.ShowUsernames:
                    result = customer.Username;
                    break;
                case CustomerNameFormat.ShowFirstName:
                    result = customer.FirstName;
                    break;
                case CustomerNameFormat.ShowNameAndCity:
                    {
                        var firstName = customer.FirstName;
                        var lastName = customer.LastName;
                        var city = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);

                        if (firstName.IsEmpty())
                        {
                            var address = customer.Addresses.FirstOrDefault();
                            if (address != null)
                            {
                                firstName = address.FirstName;
                                lastName = address.LastName;
                                city = address.City;
                            }
                        }

                        result = firstName;
                        if (lastName.HasValue())
                        {
                            result = "{0} {1}.".FormatInvariant(result, lastName.First());
                        }

                        if (city.HasValue())
                        {
                            var from = T("Common.ComingFrom");
                            result = "{0} {1} {2}".FormatInvariant(result, from, city);
                        }
                    }
                    break;
                default:
                    break;
            }

            var maxLength = customerSettings.CustomerNameFormatMaxLength;
            if (stripTooLong && maxLength > 0 && result != null && result.Length > maxLength)
            {
                result = result.Truncate(maxLength, "...");
            }

            return result;
        }

        /// <summary>
        /// Find any email address of customer
        /// </summary>
        public static string FindEmail(this Customer customer)
        {
            if (customer != null)
            {
                return customer.Email.NullEmpty()
                    ?? customer.BillingAddress?.Email?.NullEmpty()
                    ?? customer.ShippingAddress?.Email?.NullEmpty();
            }

            return null;
        }

        public static Language GetLanguage(this Customer customer)
        {
            if (customer == null)
                return null;

            var language = EngineContext.Current.Resolve<ILanguageService>().GetLanguageById(customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId));

            if (language == null || !language.Published)
            {
                language = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage;
            }

            return language;
        }

        /// <summary>
        /// Returns a random system color suitable as avatar background color.
        /// </summary>
        /// <param name="customer">Customer entity.</param>
        /// <param name="genericAttributeService">Generic attribute service.</param>
        /// <returns>Random system color.</returns>
        public static string GetAvatarColor(this Customer customer, IGenericAttributeService genericAttributeService)
        {
            Guard.NotNull(genericAttributeService, nameof(genericAttributeService));

            string color = null;

            if (customer != null)
            {
                color = customer.GetAttribute<string>(SystemCustomerAttributeNames.AvatarColor, genericAttributeService);
                if (color.IsEmpty())
                {
                    var rnd = CommonHelper.GenerateRandomInteger(0, _systemColors.Length - 1);
                    color = _systemColors[rnd];

                    genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarColor, color);
                }
            }

            return color;
        }

        #region Shopping cart

        public static int CountProductsInCart(this Customer customer, ShoppingCartType cartType, int? storeId = null)
        {
            if (customer != null)
            {
                var cartService = EngineContext.Current.Resolve<IShoppingCartService>();
                var count = cartService.CountItems(customer, cartType, storeId);

                return count;
            }

            return 0;
        }

        public static List<OrganizedShoppingCartItem> GetCartItems(this Customer customer, ShoppingCartType cartType, int? storeId = null)
        {
            if (customer != null)
            {
                var cartService = EngineContext.Current.Resolve<IShoppingCartService>();
                var items = cartService.GetCartItems(customer, cartType, storeId);

                return items;
            }

            return new List<OrganizedShoppingCartItem>();
        }

        #endregion

        #region Gift cards

        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Coupon codes</returns>
        public static string[] ParseAppliedGiftCardCouponCodes(this Customer customer)
        {
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            string existingGiftCartCouponCodes = customer.GetAttribute<string>(SystemCustomerAttributeNames.GiftCardCouponCodes,
                genericAttributeService);

            var couponCodes = new List<string>();
            if (String.IsNullOrEmpty(existingGiftCartCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingGiftCartCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string code = node1.Attributes["Code"].InnerText.Trim();
                        couponCodes.Add(code);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return couponCodes.ToArray();
        }

        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static void ApplyGiftCardCouponCode(this Customer customer, string couponCode)
        {
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            string result = string.Empty;
            try
            {
                string existingGiftCartCouponCodes = customer.GetAttribute<string>(SystemCustomerAttributeNames.GiftCardCouponCodes,
                    genericAttributeService);

                couponCode = couponCode.Trim().ToLower();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(existingGiftCartCouponCodes))
                {
                    var element1 = xmlDoc.CreateElement("GiftCardCouponCodes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(existingGiftCartCouponCodes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//GiftCardCouponCodes");

                XmlElement gcElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string couponCodeAttribute = node1.Attributes["Code"].InnerText.Trim();
                        if (couponCodeAttribute.ToLower() == couponCode.ToLower())
                        {
                            gcElement = (XmlElement)node1;
                            break;
                        }
                    }
                }

                //create new one if not found
                if (gcElement == null)
                {
                    gcElement = xmlDoc.CreateElement("CouponCode");
                    gcElement.SetAttribute("Code", couponCode);
                    rootElement.AppendChild(gcElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            //apply new value
            genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, result);
        }

        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code to remove</param>
        /// <returns>New coupon codes document</returns>
        public static void RemoveGiftCardCouponCode(this Customer customer, string couponCode)
        {
            //get applied coupon codes
            var existingCouponCodes = customer.ParseAppliedGiftCardCouponCodes();

            //clear them
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, null);

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(couponCode, StringComparison.InvariantCultureIgnoreCase))
                    customer.ApplyGiftCardCouponCode(existingCouponCode);
        }

        #endregion
    }
}
