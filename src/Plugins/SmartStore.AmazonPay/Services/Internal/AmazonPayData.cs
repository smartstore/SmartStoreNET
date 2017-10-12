using System;

namespace SmartStore.AmazonPay.Services.Internal
{
	internal class AmazonPayData
	{
		public string MessageType { get; set; }
		public string MessageId { get; set; }
		public string AuthorizationId { get; set; }
		public string CaptureId { get; set; }
		public string RefundId { get; set; }
		public string ReferenceId { get; set; }

		public string ReasonCode { get; set; }
		public string ReasonDescription { get; set; }
		public string State { get; set; }
		public DateTime StateLastUpdate { get; set; }

		public AmazonPayPrice Fee { get; set; }
		public AmazonPayPrice AuthorizedAmount { get; set; }
		public AmazonPayPrice CapturedAmount { get; set; }
		public AmazonPayPrice RefundedAmount { get; set; }

		public bool? CaptureNow { get; set; }
		public DateTime Creation { get; set; }
		public DateTime? Expiration { get; set; }

		public string AnyAmazonId
		{
			get
			{
				if (CaptureId.HasValue())
					return CaptureId;
				if (AuthorizationId.HasValue())
					return AuthorizationId;
				return RefundId;
			}
		}
	}
}