using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Payments
{
	public class PaymentMethodEditModel : ProviderModel
	{
		public string IconUrl { get; set; }
	}
}