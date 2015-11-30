using System;
using System.Globalization;

namespace SmartStore.PayPal.Services
{
	public class PayPalLineItem : ICloneable<PayPalLineItem>
	{
		public PayPalItemType Type { get; set; }
		public string Name { get; set; }
		public int Quantity { get; set; }
		public decimal Amount { get; set; }

		public decimal AmountRounded
		{
			get
			{
				return Math.Round(Amount, 2);
			}
		}

		public PayPalLineItem Clone()
		{
			var item = new PayPalLineItem()
			{
				Type = this.Type,
				Name = this.Name,
				Quantity = this.Quantity,
				Amount = this.Amount
			};
			return item;
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
	}


	public enum PayPalItemType : int
	{
		CartItem = 0,
		CheckoutAttribute,
		Shipping,
		PaymentFee,
		Tax
	}
}
