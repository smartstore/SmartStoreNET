using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Services.Payments;

namespace SmartStore.Web.Framework.Controllers
{
	public abstract class PaymentControllerBase : SmartController
    {
        public abstract IList<string> ValidatePaymentForm(FormCollection form);
        public abstract ProcessPaymentRequest GetPaymentInfo(FormCollection form);

		public virtual string GetPaymentSummary(FormCollection form)
		{
			return null;
		}
    }
}
