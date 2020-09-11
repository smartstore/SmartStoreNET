using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Payments
{
    /// <summary>
    /// Allows to filter out payment methods.
    /// </summary>
	public partial interface IPaymentMethodFilter
    {
        /// <summary>
        /// Gets a value indicating whether a payment method should be filtered out.
        /// </summary>
        /// <param name="request">Payment filter request.</param>
        /// <returns><c>true</c> filter out method, <c>false</c> do not filter out method.</returns>
        bool IsExcluded(PaymentFilterRequest request);
    }


    public partial class PaymentFilterRequest
    {
        /// <summary>
        /// The payment method to be checked
        /// </summary>
        public Provider<IPaymentMethod> PaymentMethod { get; set; }

        /// <summary>
        /// The context shopping cart
        /// </summary>
        public IList<OrganizedShoppingCartItem> Cart { get; set; }

        /// <summary>
        /// The context store identifier
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// The context customer
        /// </summary>
        public Customer Customer { get; set; }
    }
}
