using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Services
{
	public class PayPalProcessPaymentRequest : ProcessPaymentRequest
    {
        /// <summary>
        /// Gets or sets an order Discount Amount
        /// </summary>
        public decimal Discount { get; set; }
    }
}