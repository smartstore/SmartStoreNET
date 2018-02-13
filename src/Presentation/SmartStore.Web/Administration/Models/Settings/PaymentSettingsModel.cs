using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Payments;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Settings
{
	public class PaymentSettingsModel : ModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Settings.Payment.CapturePaymentReason")]
		public CapturePaymentReason? CapturePaymentReason { get; set; }
		public IList<SelectListItem> AvailableCapturePaymentReasons { get; set; }
	}
}