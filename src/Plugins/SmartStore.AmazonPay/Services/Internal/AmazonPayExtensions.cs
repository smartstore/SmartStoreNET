using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;

namespace SmartStore.AmazonPay
{
    internal static class AmazonPayExtensions
    {
        internal static void ToFirstAndLastName(this string name, out string firstName, out string lastName)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                int index = name.LastIndexOf(' ');
                if (index == -1)
                {
                    firstName = "";
                    lastName = name;
                }
                else
                {
                    firstName = name.Substring(0, index);
                    lastName = name.Substring(index + 1);
                }

                firstName = firstName.EmptyNull().Truncate(4000);
                lastName = lastName.EmptyNull().Truncate(4000);
            }
            else
            {
                firstName = lastName = "";
            }
        }

        internal static void ToFirstAndLastName(this Address address, string name)
        {
            string firstName, lastName;
            name.ToFirstAndLastName(out firstName, out lastName);

            address.FirstName = firstName;
            address.LastName = lastName;
        }

        internal static Address FindAddress(this List<Address> addresses, Address address, bool uncompleteToo)
        {
            var match = addresses.FindAddress(address.FirstName, address.LastName,
                address.PhoneNumber, address.Email, address.FaxNumber, address.Company,
                address.Address1, address.Address2,
                address.City, address.StateProvinceId, address.ZipPostalCode, address.CountryId);

            if (match == null && uncompleteToo)
            {
                // Compare with ToAddress
                match = addresses.FirstOrDefault(x =>
                    x.FirstName == null && x.LastName == null &&
                    x.Address1 == null && x.Address2 == null &&
                    x.City == address.City && x.ZipPostalCode == address.ZipPostalCode &&
                    x.PhoneNumber == null &&
                    x.CountryId == address.CountryId && x.StateProvinceId == address.StateProvinceId
                );
            }

            return match;
        }

        internal static string ToAmazonLanguageCode(this string twoLetterLanguageCode, char delimiter = '-')
        {
            switch (twoLetterLanguageCode.EmptyNull().ToLower())
            {
                case "en":
                    return $"en{delimiter}GB";
                case "fr":
                    return $"fr{delimiter}FR";
                case "it":
                    return $"it{delimiter}IT";
                case "es":
                    return $"es{delimiter}ES";
                case "de":
                default:
                    return $"de{delimiter}DE";
            }
        }

        internal static bool HasAmazonPayState(this HttpContextBase httpContext)
        {
            var checkoutState = httpContext.GetCheckoutState();
            var checkoutStateKey = AmazonPayPlugin.SystemName + ".CheckoutState";

            if (checkoutState != null && checkoutState.CustomProperties.ContainsKey(checkoutStateKey))
            {
                var state = checkoutState.CustomProperties[checkoutStateKey] as AmazonPayCheckoutState;

                return state != null && state.AccessToken.HasValue();
            }

            return false;
        }

        internal static AmazonPayCheckoutState GetAmazonPayState(this HttpContextBase httpContext, ILocalizationService localizationService)
        {
            var checkoutState = httpContext.GetCheckoutState();

            if (checkoutState == null)
                throw new SmartException(localizationService.GetResource("Plugins.Payments.AmazonPay.MissingCheckoutSessionState"));

            var state = checkoutState.CustomProperties.Get(AmazonPayPlugin.SystemName + ".CheckoutState") as AmazonPayCheckoutState;

            if (state == null)
                throw new SmartException(localizationService.GetResource("Plugins.Payments.AmazonPay.MissingCheckoutSessionState"));

            return state;
        }

        internal static Order GetOrderByAmazonId(this IRepository<Order> orderRepository, string amazonId)
        {
            // S02-9777218-8608106				OrderReferenceId
            // S02-9777218-8608106-A088344		Auth ID
            // S02-9777218-8608106-C088344		Capture ID

            if (amazonId.HasValue())
            {
                string amazonOrderReferenceId = amazonId.Substring(0, amazonId.LastIndexOf('-'));
                if (amazonOrderReferenceId.HasValue())
                {
                    var orders = orderRepository.Table
                        .Where(x => x.PaymentMethodSystemName == AmazonPayPlugin.SystemName && x.AuthorizationTransactionId.StartsWith(amazonOrderReferenceId))
                        .ToList();

                    if (orders.Count() == 1)
                        return orders.FirstOrDefault();
                }
            }
            return null;
        }
    }
}