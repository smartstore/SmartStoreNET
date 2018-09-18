using System;
using System.Collections.Generic;
using AmazonPay;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.AmazonPay.Services
{
	[Serializable]
	public class AmazonPayCheckoutState
	{
		public string OrderReferenceId { get; set; }
		public string AccessToken { get; set; }
	}


	public class AmazonPayActionState
	{
		public Guid OrderGuid { get; set; }
		public List<string> Errors { get; set; }
	}


	[Serializable]
	public class AmazonPayOrderAttribute
	{
		public string OrderReferenceId { get; set; }
		public bool OrderReferenceClosed { get; set; }
	}


	internal class PollingLoopData
	{
		public PollingLoopData(int orderId)
		{
			OrderId = orderId;
		}

		public int OrderId { get; private set; }
		public Order Order { get; set; }
		public AmazonPaySettings Settings { get; set; }
		public Client Client { get; set; }
	}
}
