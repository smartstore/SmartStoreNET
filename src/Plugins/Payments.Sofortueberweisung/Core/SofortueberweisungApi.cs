using System;
using System.Net;
using System.Text;
using System.Xml;
using SmartStore.Core;
using System.IO;
using SmartStore.Services.Payments;
using System.Globalization;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Payments;
using SmartStore.Services.Orders;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Logging;
using System.Web;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Plugin.Payments.Sofortueberweisung.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Plugin.Payments.Sofortueberweisung.Core
{
	public class SofortueberweisungApi : ISofortueberweisungApi
	{
		private const string _apiUrl = "https://api.sofort.com/api/xml";
		private const string _contentType = "application/xml; charset=UTF-8";
		private const string _transactionRequestVersion = "2";

		private readonly PluginHelperBase _helper;
		private readonly IWebHelper _webHelper;
		private readonly PaymentSettings _paymentSettings;
		private readonly SofortueberweisungPaymentSettings _paymentSettingsSu;
		private readonly IStoreContext _storeContext;
		private readonly IPaymentService _paymentService;
		private readonly IOrderService _orderService;
		private readonly IOrderProcessingService _orderProcessingService;
		private readonly ILogger _logger;
		private bool _processorLoaded;
		private SofortueberweisungPaymentProcessor _processor;
		private string[] _configKey;

		public SofortueberweisungApi(
			IWebHelper webHelper,
			PaymentSettings paymentSettings,
			SofortueberweisungPaymentSettings paymentSettingsSu,
			IStoreContext storeContext,
			IPaymentService paymentService,
			IOrderService orderService,
			IOrderProcessingService orderProcessingService,
			ILogger logger)
		{
			_webHelper = webHelper;
			_paymentSettings = paymentSettings;
			_paymentSettingsSu = paymentSettingsSu;
			_storeContext = storeContext;
			_paymentService = paymentService;
			_orderService = orderService;
			_orderProcessingService = orderProcessingService;
			_logger = logger;

			_helper = new PluginHelperBase("Payments.Sofortueberweisung");
		}

		private string[] ConfigKey {
			get {
				if (_configKey == null)
					_configKey = _paymentSettingsSu.ApiConfigKey.SplitSafe(":");
				return _configKey;
			}
		}
		private bool ValidCredentials {
			get {
				return (ConfigKey.Length == 3 && ConfigKey[0].HasValue() && ConfigKey[1].HasValue() && ConfigKey[2].HasValue());
			}
		}
		public PluginHelperBase Helper { get { return _helper; } }
		public SofortueberweisungPaymentProcessor Processor
		{
			get
			{
				if (!_processorLoaded)
				{
					_processorLoaded = true;
					var obj = _paymentService.LoadPaymentMethodBySystemName(Helper.SystemName) as SofortueberweisungPaymentProcessor;
					_processor = (obj != null && obj.IsPaymentMethodActive(_paymentSettings) && obj.PluginDescriptor.Installed ? obj : null);
				}
				return _processor;
			}
		}

		private bool LogErrors(XmlNodeList lst, bool isWarning) {
			bool hasError = false;

			if (lst != null) {
				foreach (XmlNode node in lst) {
					string msg = "{0} ({1}, {2})".FormatWith(node.GetText("message"), node.GetText("code"), node.GetText("field"));

					if (isWarning) {
						_logger.Warning(Helper.Resource("SuReportsWarning") + msg);
					}
					else {
						hasError = true;
						_logger.Error(Helper.Resource("SuReportsError") + msg);
					}
				}
			}
			return hasError;
		}
		private bool LogErrors(XmlDocument doc) {
			bool hasError = false;

			try {
				if (doc != null && doc.DocumentElement != null) {
					if (LogErrors(doc.SelectNodes("//errors/error"), false))
						hasError = true;

					if (LogErrors(doc.SelectNodes("//su/errors/error"), false))
						hasError = true;

					if (_paymentSettingsSu.SaveWarnings) {
						LogErrors(doc.SelectNodes("//warnings/warning"), true);
						LogErrors(doc.SelectNodes("//su/warnings/warning"), true);
					}
				}
			}
			catch (Exception exc) {
				exc.Dump();
			}
			return hasError;
		}
		private bool Send(XmlDocument docIn, out XmlDocument docOut) {
			docOut = null;
			bool result = false;

			try {
				if (ValidCredentials) {
					byte[] bytes = Encoding.ASCII.GetBytes(docIn.InnerXml);

					var request = (HttpWebRequest)WebRequest.Create(_apiUrl);
					request.Method = "POST";
					request.Accept = _contentType;
					request.ContentType = _contentType;
					request.ContentLength = bytes.Length;

					request.Credentials = new NetworkCredential(ConfigKey[0], ConfigKey[2]);
					//string authInfo = userName + ":" + userPassword;
					//authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
					//request.Headers["Authorization"] = "Basic " + authInfo;

					using (var stream = request.GetRequestStream()) {
						stream.Write(bytes, 0, bytes.Length);
						stream.Close();
					}

					using (var response = (HttpWebResponse)request.GetResponse())
					using (var stream = new StreamReader(response.GetResponseStream())) {
						string xml = stream.ReadToEnd();
						docOut = new XmlDocument();
						docOut.LoadXml(xml);

						if (!LogErrors(docOut))
							result = true;

						stream.Close();
						response.Close();
					}					
				}
				else {
					_logger.Error(Helper.Resource("InvalidCredentials"));
				}
			}
			catch (Exception exc) {
				exc.Dump();
				_logger.Error(Helper.Resource("SendFailed"), exc);
			}
			return result;
		}
		private PaymentStatus GetPaymentStatus(string status, string statusReason)
		{
			if (status == null)
				status = "";
			if (statusReason == null)
				statusReason = "";

			switch (status.ToLowerInvariant())
			{
				case "loss":
					return PaymentStatus.Voided;
				case "received":
					return PaymentStatus.Paid;
				case "refunded":
					return (statusReason.IsCaseInsensitiveEqual("compensation") ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded);
				case "untraceable":
					return PaymentStatus.Pending; //?
				default:
					return PaymentStatus.Pending;
			}
		}
		private bool ParseOrderTotal(string total, out decimal result) {
			if (decimal.TryParse(total, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
				return true;
			_logger.Error(Helper.Resource("ParseTotalFailed").FormatWith(total ?? "-"));
			return false;
		}
		private string GetPaymentReason(PostProcessPaymentRequest payment, bool first)
		{
			string setting = (first ? _paymentSettingsSu.PaymentReason1 : _paymentSettingsSu.PaymentReason2);

			if (setting.IsNullOrEmpty())
				setting = (first ? DefaultPaymentReason1 : DefaultPaymentReason2);

			switch (setting)
			{
				case "Order [StoreName]":
					return "{0} {1}".FormatWith(Helper.Resource("Order"), _storeContext.CurrentStore.Name);
				case "[OrderID], [OrderDate]":
					return "{0}, {1}".FormatWith(payment.Order.Id, payment.Order.CreatedOnUtc.ToShortDateString());
				case "[OrderID], [StoreID]":
					return "{0}, {1}".FormatWith(payment.Order.Id, _storeContext.CurrentStore.Id);
				case "[OrderID], [StoreName]":
					return "{0}, {1}".FormatWith(payment.Order.Id, _storeContext.CurrentStore.Name);
				case "[OrderID]":
					return payment.Order.Id.ToString();
				case "[OrderDate]":
					return payment.Order.CreatedOnUtc.ToShortDateString();
				case "[StoreID]":
					return _storeContext.CurrentStore.Id.ToString();
				case "[StoreName]":
					return _storeContext.CurrentStore.Name;
				default:
					return setting;
			}
		}

		public static string DefaultPaymentReason1 { get { return "Order [StoreName]"; } }
		public static string DefaultPaymentReason2 { get { return "[OrderID], [OrderDate]"; } }
		public static string FurtherInfoUrl { get { return "https://documents.sofort.com/sue/kundeninformationen/"; } }
		public static string CustomerProtectionInfoUrl { get { return "https://www.sofort-bank.com/ger-DE/general/kaeuferschutz/informationen-fuer-kaeufer/"; } }

		public bool PaymentProcess(ProcessPaymentResult result)
		{
			if (!ValidCredentials)
			{
				result.AddError(Helper.Resource("InvalidCredentials"));
				return false;
			}
			return true;
		}
		public bool PaymentInitiate(PostProcessPaymentRequest payment, out string transactionID, out string paymentUrl)
		{
			transactionID = paymentUrl = "";

			XmlDocument docOut;
			var allowedChars = new char[] { '+', ',', '-', '.' };
			string orderTotal = Math.Round(payment.Order.OrderTotal, 2).ToString("0.00", CultureInfo.InvariantCulture);
			string reason1 = GetPaymentReason(payment, true);
			string reason2 = GetPaymentReason(payment, false);
			string userData = "{0}|{1}".FormatWith(payment.Order.Id, payment.Order.OrderGuid.ToString());
			string pluginUrl = "{0}Plugins/PaymentSofortueberweisung/{{0}}".FormatWith(_webHelper.GetStoreLocation(false));

			var docIn = Helper.CreateXmlDocument(xw =>
			{
				xw.WriteStartElement("multipay");

				xw.WriteElementString("project_id", ConfigKey[1]);
				xw.WriteElementString("language_code", Helper.Language.UniqueSeoCode ?? "DE");
				xw.WriteElementString("interface_version", Helper.InterfaceVersion);
				xw.WriteElementString("amount", orderTotal);
				xw.WriteElementString("currency_code", Helper.CurrencyCode);
				xw.WriteElementString("success_url", pluginUrl.FormatWith("Success"));
				xw.WriteElementString("abort_url", pluginUrl.FormatWith("Abort"));
			
				xw.WriteStartElement("reasons");
				xw.WriteElementString("reason", reason1.Prettify(true, allowedChars).Truncate(27));
				xw.WriteElementString("reason", reason2.Prettify(true, allowedChars).Truncate(27));
				xw.WriteEndElement();

				if (_paymentSettingsSu.UseTestAccount)
				{
					xw.WriteStartElement("sender");
					xw.WriteElementString("holder", _paymentSettingsSu.AccountHolder);
					xw.WriteElementString("account_number", _paymentSettingsSu.AccountNumber);
					xw.WriteElementString("bank_code", _paymentSettingsSu.AccountBankCode);
					xw.WriteElementString("country_code", _paymentSettingsSu.AccountCountry);
					xw.WriteEndElement();
				}

				xw.WriteStartElement("user_variables");
				xw.WriteElementString("user_variable", userData);	// max. 255 chars... prefer to use one rather than multiple variables
				xw.WriteEndElement();

				xw.WriteStartElement("notification_urls");
				xw.WriteStartElement("notification_url");
				xw.WriteAttributeString("notify_on", "loss,pending,received,refunded,untraceable");
				xw.WriteValue(pluginUrl.FormatWith("Notification"));
				xw.WriteEndElement();
				xw.WriteEndElement();
				
				xw.WriteStartElement("su");

				if (_paymentSettingsSu.CustomerProtection)
				{
					xw.WriteElementString("customer_protection", "1");
				}
				xw.WriteEndElement();
				
				xw.WriteEndElement();
				return true;
			});

			if (Send(docIn, out docOut))
			{
				transactionID = docOut.GetText("//transaction");
				paymentUrl = docOut.GetText("//payment_url");

				return paymentUrl.HasValue();
			}
			return false;
		}
		public bool PaymentDetails(string transactionID)
		{
			if (transactionID.HasValue())
			{
				decimal orderTotal;
				XmlDocument docOut;
				XmlNode node;
				Order order;

				var docIn = Helper.CreateXmlDocument(xw =>
				{
					xw.WriteStartElement("transaction_request");
					xw.WriteAttributeString("version", _transactionRequestVersion);
					xw.WriteElementString("transaction", transactionID);
					xw.WriteEndElement();
					return true;
				});

				if (Send(docIn, out docOut) && (node = docOut.SelectSingleNode("//transaction_details[1]")) != null)
				{
					var paymentStatus = GetPaymentStatus(node.GetText("status"), node.GetText("status_reason"));
					var userData = new SofortueberweisungUserData(node.GetText("user_variables/user_variable[1]"));
					
					if (userData.OrderGuid != null && (order = _orderService.GetOrderByGuid(userData.OrderGuid)) != null)
					{
						bool testTransaction = (node.GetText("test", "0") == "1");

						string note = Helper.Resource("PaymentNote").FormatWith(
							node.GetText("reasons/reason[1]", "-"),
							transactionID,
							node.GetText("status"), node.GetText("status_reason"),
							node.GetText("currency_code"), node.GetText("amount"), node.GetText("amount_refunded", "-"),
							node.GetText("payment_method"),
							node.GetText("costs/currency_code"), node.GetText("costs/fees"),
							node.GetText("email_customer", "-"),
							node.GetText("phone_customer", "-"),
							node.GetText("/sender/holder", "-"),
							node.GetText("/sender/account_number", "-"),
							node.GetText("/sender/bank_code", "-"), node.GetText("/sender/bank_name", "-"),
							node.GetText("/sender/iban", "-"), node.GetText("/sender/bic", "-")
						);

						order.OrderNotes.Add(new OrderNote()
						{
							Note = note,
							DisplayToCustomer = false,
							CreatedOnUtc = DateTime.UtcNow
						});
						_orderService.UpdateOrder(order);

						if (_paymentSettingsSu.ValidateOrderTotal && ParseOrderTotal(node.GetText("amount"), out orderTotal) && !Math.Round(orderTotal, 2).Equals(Math.Round(order.OrderTotal, 2)))
						{
							_logger.Error(Helper.Resource("OrderTotalUnequal").FormatWith(orderTotal, order.OrderTotal));
							return false;
						}

						if ((paymentStatus == PaymentStatus.Paid || testTransaction) && _orderProcessingService.CanMarkOrderAsPaid(order))
						{
							order.AuthorizationTransactionId = transactionID;
							_orderService.UpdateOrder(order);

							_orderProcessingService.MarkOrderAsPaid(order);
						}
						else if (paymentStatus == PaymentStatus.Voided && _orderProcessingService.CanVoidOffline(order))
						{
							_orderProcessingService.VoidOffline(order);
						}
						else if (paymentStatus == PaymentStatus.Refunded && _orderProcessingService.CanRefundOffline(order))
						{
							_orderProcessingService.RefundOffline(order);
						}
						return true;
					}
				}
			}
			return false;
		}
		public bool PaymentDetails(HttpRequestBase request)
		{
			if (Processor == null)
			{
				_logger.Error(Helper.Resource("ModuleFailed"));
				return false;
			}

			string transaction = null;
			try
			{
				using (var reader = new StreamReader(request.InputStream))
				{
					var doc = new XmlDocument();
					doc.Load(reader);

					transaction = doc.GetText("//transaction");
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
				_logger.Error(Helper.Resource("ReadNotificationFailed"), exc);
			}
			return PaymentDetails(transaction);
		}
		public void SetupConfigurationModel(ConfigurationModel model)
		{
			model.AvailablePaymentReason = new List<SelectListItem>()
			{
				new SelectListItem { Text = "{0} [StoreName]".FormatWith(Helper.Resource("Order")), Value = "Order [StoreName]" },
				new SelectListItem { Text = "[OrderID], [OrderDate]", Value = "[OrderID], [OrderDate]" },
				new SelectListItem { Text = "[OrderID], [StoreID]", Value = "[OrderID], [StoreID]" },
				new SelectListItem { Text = "[OrderID], [StoreName]", Value = "[OrderID], [StoreName]" },
				new SelectListItem { Text = "[OrderID]", Value = "[OrderID]" },
				new SelectListItem { Text = "[OrderDate]", Value = "[OrderDate]" },
				new SelectListItem { Text = "[StoreID]", Value = "[StoreID]" },
				new SelectListItem { Text = "[StoreName]", Value = "[StoreName]" }
			};

			if (model.PaymentReason1.IsNullOrEmpty())
				model.PaymentReason1 = DefaultPaymentReason1;

			if (model.PaymentReason2.IsNullOrEmpty())
				model.PaymentReason2 = DefaultPaymentReason2;
		}
	}	// class


	public class SofortueberweisungUserData
	{
		public SofortueberweisungUserData()
		{
		}
		public SofortueberweisungUserData(string userData)
		{
			try
			{
				string[] data = userData.SplitSafe("|");
				if (data.Length >= 2)
				{
					OrderID = int.Parse(data[0]);
					OrderGuid = new Guid(data[1]);
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
		}

		public int OrderID { get; set; }
		public Guid OrderGuid { get; set; }
	}	// class
}
