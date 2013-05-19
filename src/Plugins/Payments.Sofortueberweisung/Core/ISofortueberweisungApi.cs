using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Plugin.Payments.Sofortueberweisung.Core
{
	public partial interface ISofortueberweisungApi
	{
		PluginHelperBase Helper { get; }
		SofortueberweisungPaymentProcessor Processor { get; }

		bool PaymentProcess(ProcessPaymentResult result);
		bool PaymentInitiate(PostProcessPaymentRequest payment, out string transactionID, out string paymentUrl);
		bool PaymentDetails(string transactionID);
		bool PaymentDetails(HttpRequestBase request);
	}
}
