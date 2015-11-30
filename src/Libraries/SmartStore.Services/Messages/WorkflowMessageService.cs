using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILanguageService _languageService;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
		private readonly IStoreService _storeService;
		private readonly IStoreContext _storeContext;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;
        private readonly HttpRequestBase _httpRequest;
		private readonly IDownloadService _downloadServioce;

        #endregion

        #region Ctor

        public WorkflowMessageService(
			IMessageTemplateService messageTemplateService,
            IQueuedEmailService queuedEmailService, 
			ILanguageService languageService,
            ITokenizer tokenizer, 
			IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
			IStoreService storeService,
			IStoreContext storeContext,
            EmailAccountSettings emailAccountSettings,
            IEventPublisher eventPublisher,
            IWorkContext workContext,
            HttpRequestBase httpRequest,
			IDownloadService downloadServioce)
        {
            this._messageTemplateService = messageTemplateService;
            this._queuedEmailService = queuedEmailService;
            this._languageService = languageService;
            this._tokenizer = tokenizer;
            this._emailAccountService = emailAccountService;
            this._messageTokenProvider = messageTokenProvider;
			this._storeService = storeService;
			this._storeContext = storeContext;
            this._emailAccountSettings = emailAccountSettings;
            this._eventPublisher = eventPublisher;
            this._workContext = workContext;
            this._httpRequest = httpRequest;
			this._downloadServioce = downloadServioce;
        }

        #endregion

        #region Utilities

        protected int SendNotification(
			MessageTemplate messageTemplate,
            EmailAccount emailAccount, 
			int languageId, 
			IList<Token> tokens,
            string toEmailAddress, 
			string toName,
			string replyTo = null,
			string replyToName = null)
        {
            // retrieve localized message template data
            var bcc = messageTemplate.GetLocalized((mt) => mt.BccEmailAddresses, languageId);
            var subject = messageTemplate.GetLocalized((mt) => mt.Subject, languageId);
            var body = messageTemplate.GetLocalized((mt) => mt.Body, languageId);
			
            // Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
			
            bodyReplaced = WebHelper.MakeAllUrlsAbsolute(bodyReplaced, _httpRequest);
			
            var email = new QueuedEmail
            {
                Priority = 5,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                CC = string.Empty,
                Bcc = bcc,
				ReplyTo = replyTo,
				ReplyToName = replyToName,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
				SendManually = messageTemplate.SendManually
            };

			// create attachments if any
			var fileIds = (new int?[] 
				{ 
					messageTemplate.GetLocalized(x => x.Attachment1FileId, languageId),
					messageTemplate.GetLocalized(x => x.Attachment2FileId, languageId),
					messageTemplate.GetLocalized(x => x.Attachment3FileId, languageId)
				})
				.Where(x => x.HasValue)
				.Select(x => x.Value)
				.ToArray();

			if (fileIds.Any())
			{
				var files = _downloadServioce.GetDownloadsByIds(fileIds);
				foreach (var file in files)
				{
					email.Attachments.Add(new QueuedEmailAttachment
					{
						StorageLocation = EmailAttachmentStorageLocation.FileReference,
						FileId = file.Id,
						Name = (file.Filename.NullEmpty() ?? file.Id.ToString()) + file.Extension.EmptyNull(),
						MimeType = file.ContentType.NullEmpty() ?? "application/octet-stream"
					});
				}
			}
			

			// publish event so that integrators can add attachments, alter the email etc.
			_eventPublisher.Publish(new QueuingEmailEvent
			{
				EmailAccount = emailAccount,
				LanguageId = languageId,
				MessageTemplate = messageTemplate,
				QueuedEmail = email,
				Tokens = tokens
			});

            _queuedEmailService.InsertQueuedEmail(email);

            return email.Id;
        }

        protected MessageTemplate GetActiveMessageTemplate(string messageTemplateName, int storeId)
        {
			var messageTemplate = _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);

			if (messageTemplate == null)
				return null;

			//ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        protected EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccounId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccounId);
            if (emailAccount == null)
				emailAccount = _emailAccountService.GetDefaultEmailAccount();

            return emailAccount;
        }

        private Tuple<string, string> GetReplyToEmail(Customer customer)
        {
			if (customer == null || customer.Email.IsEmpty())
				return new Tuple<string, string>(null, null);

			string email = customer.Email;
			string name = GetDisplayNameForCustomer(customer);

			return new Tuple<string, string>(email, name);
        }

        private string GetDisplayNameForCustomer(Customer customer)
        {
            if (customer == null)
                return string.Empty;

            Func<Address, string> getName = (address) => {
                if (address == null)
                    return null;

                string result = string.Empty;
                if (address.FirstName.HasValue() || address.LastName.HasValue())
                {
                    result = string.Format("{0} {1}", address.FirstName, address.LastName).Trim();
                }

                if (address.Company.HasValue())
                {
                    result = string.Concat(result, result.HasValue() ? ", " : "", address.Company);
                }

                return result;
            };

            string name = getName(customer.BillingAddress);
            if (name.IsEmpty())
            {
                name = getName(customer.ShippingAddress);
            }
            if (name.IsEmpty())
            {
                name = getName(customer.Addresses.FirstOrDefault());
            }

            name = name.TrimSafe().NullEmpty();

            return name ?? customer.Username.EmptyNull();
        }

		protected int EnsureLanguageIsActive(int languageId, int storeId)
        {
			//load language by specified ID
            var language = _languageService.GetLanguageById(languageId);

            if (language == null || !language.Published)
			{
				//load any language from the specified store
				language = _languageService.GetAllLanguages(storeId: storeId).FirstOrDefault();
			}
			if (language == null || !language.Published)
			{
				//load any language
				language = _languageService.GetAllLanguages().FirstOrDefault();
			}

			if (language == null)
				throw new Exception("No active language could be loaded");
            return language.Id;
        }

        #endregion

        #region Methods

        #region Customer workflow

        /// <summary>
        /// Sends 'New customer' notification message to a store owner
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerRegisteredNotificationMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("NewCustomer.Notification", store.Id);
            if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
			_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            // use customer email as reply address
			var replyTo = GetReplyToEmail(customer);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
				replyTo.Item1, replyTo.Item2);
        }

        /// <summary>
        /// Sends a welcome message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerWelcomeMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("Customer.WelcomeMessage", store.Id);
            if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
			_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an email validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerEmailValidationMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("Customer.EmailValidationMessage", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
			_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerPasswordRecoveryMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("Customer.PasswordRecovery", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
			_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Order workflow

        /// <summary>
        /// Sends an order placed notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPlacedStoreOwnerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

			var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("OrderPlaced.StoreOwnerNotification", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddOrderTokens(tokens, order, languageId);
			_messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            // use buyer's email as reply address
            var replyToEmail = order.BillingAddress.Email;
            var replyToName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            if (order.BillingAddress.Company.HasValue())
            {
				replyToName += ", " + order.BillingAddress.Company;
            }

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
				replyToEmail, replyToName);
        }

        /// <summary>
        /// Sends an order placed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPlacedCustomerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

			var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("OrderPlaced.CustomerNotification", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddOrderTokens(tokens, order, languageId);
			_messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            _messageTokenProvider.AddCompanyTokens(tokens);
            _messageTokenProvider.AddBankConnectionTokens(tokens);
            _messageTokenProvider.AddContactDataTokens(tokens);
            
            // event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a shipment sent notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendShipmentSentCustomerNotification(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new Exception("Order cannot be loaded");

			var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("ShipmentSent.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
			_messageTokenProvider.AddOrderTokens(tokens, shipment.Order, languageId);
			_messageTokenProvider.AddCustomerTokens(tokens, shipment.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a shipment delivered notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendShipmentDeliveredCustomerNotification(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new Exception("Order cannot be loaded");

			var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("ShipmentDelivered.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
			_messageTokenProvider.AddOrderTokens(tokens, shipment.Order, languageId);
			_messageTokenProvider.AddCustomerTokens(tokens, shipment.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order completed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderCompletedCustomerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

			var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("OrderCompleted.CustomerNotification", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddOrderTokens(tokens, order, languageId);
			_messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            _messageTokenProvider.AddCompanyTokens(tokens);
            _messageTokenProvider.AddBankConnectionTokens(tokens);
            _messageTokenProvider.AddContactDataTokens(tokens);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order cancelled notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderCancelledCustomerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

			var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("OrderCancelled.CustomerNotification", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddOrderTokens(tokens, order, languageId);
			_messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a new order note added notification to a customer
        /// </summary>
        /// <param name="orderNote">Order note</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewOrderNoteAddedCustomerNotification(OrderNote orderNote, int languageId)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            var order = orderNote.Order;

			var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("Customer.NewOrderNote", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddOrderNoteTokens(tokens, orderNote);
			_messageTokenProvider.AddOrderTokens(tokens, orderNote.Order, languageId);
			_messageTokenProvider.AddCustomerTokens(tokens, orderNote.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "Recurring payment cancelled" notification to a store owner
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendRecurringPaymentCancelledStoreOwnerNotification(RecurringPayment recurringPayment, int languageId)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

			var store = _storeService.GetStoreById(recurringPayment.InitialOrder.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("RecurringPaymentCancelled.StoreOwnerNotification", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddOrderTokens(tokens, recurringPayment.InitialOrder, languageId);
			_messageTokenProvider.AddCustomerTokens(tokens, recurringPayment.InitialOrder.Customer);
			_messageTokenProvider.AddRecurringPaymentTokens(tokens, recurringPayment);
            
            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Newsletter workflow

        /// <summary>
        /// Sends a newsletter subscription activation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription,
            int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("NewsLetterSubscription.ActivationMessage", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a newsletter subscription deactivation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsLetterSubscriptionDeactivationMessage(NewsLetterSubscription subscription,
            int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

			var store = _storeContext.CurrentStore;
			languageId = EnsureLanguageIsActive(languageId, store.Id);
			
            var messageTemplate = GetActiveMessageTemplate("NewsLetterSubscription.DeactivationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, _storeContext.CurrentStore);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Send a message to a friend

        /// <summary>
        /// Sends "email a friend" message
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product instance</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendProductEmailAFriendMessage(Customer customer, int languageId,
            Product product, string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

			var store = _storeContext.CurrentStore;
			languageId = EnsureLanguageIsActive(languageId, store.Id);
            
			var messageTemplate = GetActiveMessageTemplate("Service.EmailAFriend", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, customer);
			_messageTokenProvider.AddProductTokens(tokens, product, languageId);
			tokens.Add(new Token("EmailAFriend.PersonalMessage", personalMessage, true));
			tokens.Add(new Token("EmailAFriend.Email", customerEmail));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        public virtual int SendProductQuestionMessage(Customer customer, int languageId, Product product, 
            string senderEmail, string senderName, string senderPhone, string question)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (customer.IsSystemAccount)
                return 0;

            if (product == null)
                throw new ArgumentNullException("product");
			
			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);
			
            var messageTemplate = GetActiveMessageTemplate("Product.AskQuestion", store.Id);
            if (messageTemplate == null)
                return 0;

			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, customer);
			_messageTokenProvider.AddProductTokens(tokens, product, languageId);

			tokens.Add(new Token("ProductQuestion.Message", question, true));
            tokens.Add(new Token("ProductQuestion.SenderEmail", senderEmail));
            tokens.Add(new Token("ProductQuestion.SenderName", senderName));
            tokens.Add(new Token("ProductQuestion.SenderPhone", senderPhone));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName, senderEmail, senderName);
        }

        /// <summary>
        /// Sends wishlist "email a friend" message
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendWishlistEmailAFriendMessage(Customer customer, int languageId,
             string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
			languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("Wishlist.EmailAFriend", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, customer);
			tokens.Add(new Token("Wishlist.PersonalMessage", personalMessage, true));
			tokens.Add(new Token("Wishlist.Email", customerEmail));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Return requests

        /// <summary>
        /// Sends 'New Return Request' message to a store owner
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewReturnRequestStoreOwnerNotification(ReturnRequest returnRequest, OrderItem orderItem, int languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

			var store = _storeService.GetStoreById(orderItem.Order.StoreId) ?? _storeContext.CurrentStore;
			languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("NewReturnRequest.StoreOwnerNotification", store.Id);
			if (messageTemplate == null)
                return 0;
            
			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, returnRequest.Customer);
			_messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, orderItem);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            // use customer email as reply address
			var replyTo = GetReplyToEmail(returnRequest.Customer);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
				replyTo.Item1, replyTo.Item2);
        }

        /// <summary>
        /// Sends 'Return Request status changed' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendReturnRequestStatusChangedCustomerNotification(ReturnRequest returnRequest, OrderItem orderItem, int languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

			var store = _storeService.GetStoreById(orderItem.Order.StoreId) ?? _storeContext.CurrentStore;
			languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("ReturnRequestStatusChanged.CustomerNotification", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, returnRequest.Customer);
			_messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, orderItem);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = returnRequest.Customer.FindEmail();
            var toName = returnRequest.Customer.GetFullName();

			if (toEmail.IsEmpty())
				return 0;

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        #endregion

        #region Forum Notifications

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendNewForumTopicMessage(Customer customer,
            ForumTopic forumTopic, Forum forum, int languageId)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }

			var store = _storeContext.CurrentStore;

			var messageTemplate = GetActiveMessageTemplate("Forums.NewForumTopic", store.Id);
			if (messageTemplate == null)
			{
				return 0;
			}

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
			_messageTokenProvider.AddForumTopicTokens(tokens, forumTopic);
			_messageTokenProvider.AddForumTokens(tokens, forumTopic.Forum, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="forumPost">Forum post</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendNewForumPostMessage(Customer customer,
            ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex, int languageId)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }

			var store = _storeContext.CurrentStore;

			var messageTemplate = GetActiveMessageTemplate("Forums.NewForumPost", store.Id);
            if (messageTemplate == null)
            {
                return 0;
            }

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddForumPostTokens(tokens, forumPost);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
			_messageTokenProvider.AddForumTopicTokens(tokens, forumPost.ForumTopic, friendlyForumTopicPageIndex, forumPost.Id);
			_messageTokenProvider.AddForumTokens(tokens, forumPost.ForumTopic.Forum, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendPrivateMessageNotification(Customer customer, PrivateMessage privateMessage, int languageId)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

			var store = _storeService.GetStoreById(privateMessage.StoreId) ?? _storeContext.CurrentStore;

			var messageTemplate = GetActiveMessageTemplate("Customer.NewPM", store.Id);
            if (messageTemplate == null)
            {
                return 0;
            }

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
			_messageTokenProvider.AddPrivateMessageTokens(tokens, privateMessage);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = privateMessage.ToCustomer.Email;
            var toName = privateMessage.ToCustomer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        #endregion

        #region Misc

        public virtual int SendGenericMessage(string messageTemplateName, Action<GenericMessageContext> cfg)
        {
            Guard.ArgumentNotNull(() => cfg);
            Guard.ArgumentNotEmpty(() => messageTemplateName);

            var ctx = new GenericMessageContext();
            ctx.MessagenTokenProvider = _messageTokenProvider;

            cfg(ctx);

            if (!ctx.StoreId.HasValue)
            {
                ctx.StoreId = _storeContext.CurrentStore.Id;
            }

            if (!ctx.LanguageId.HasValue)
            {
                ctx.LanguageId = _workContext.WorkingLanguage.Id;
            }

            if (ctx.Customer == null)
            {
                ctx.Customer = _workContext.CurrentCustomer;
            }

            if (ctx.Customer.IsSystemAccount)
                return 0;

            _messageTokenProvider.AddCustomerTokens(ctx.Tokens, ctx.Customer);
            _messageTokenProvider.AddStoreTokens(ctx.Tokens, _storeService.GetStoreById(ctx.StoreId.Value));

            ctx.LanguageId = EnsureLanguageIsActive(ctx.LanguageId.Value, ctx.StoreId.Value);

            var messageTemplate = GetActiveMessageTemplate(messageTemplateName, ctx.StoreId.Value);
            if (messageTemplate == null)
                return 0;

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, ctx.LanguageId.Value);
            var toEmail = ctx.ToEmail.HasValue() ? ctx.ToEmail : emailAccount.Email;
            var toName = ctx.ToName.HasValue() ? ctx.ToName : emailAccount.DisplayName;

            if (ctx.ReplyToCustomer && ctx.Customer != null)
            {
                // use customer email as reply address
				var replyTo = GetReplyToEmail(ctx.Customer);
				ctx.ReplyToEmail = replyTo.Item1;
				ctx.ReplyToName = replyTo.Item2;
            }

			return SendNotification(messageTemplate, emailAccount, ctx.LanguageId.Value, ctx.Tokens, toEmail, toName, ctx.ReplyToEmail, ctx.ReplyToName);
        }

        /// <summary>
        /// Sends a gift card notification
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendGiftCardNotification(GiftCard giftCard, int languageId)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

			Store store = null;
			var order = giftCard.PurchasedWithOrderItem != null ?
				giftCard.PurchasedWithOrderItem.Order :
				null;
			if (order != null)
				store = _storeService.GetStoreById(order.StoreId);
			if (store == null)
				store = _storeContext.CurrentStore;

			languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("GiftCard.Notification", store.Id);
			if (messageTemplate == null)
				return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddGiftCardTokens(tokens, giftCard);
            
            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = giftCard.RecipientEmail;
            var toName = giftCard.RecipientName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a product review notification message to a store owner
        /// </summary>
        /// <param name="productReview">Product review</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendProductReviewNotificationMessage(ProductReview productReview,
            int languageId)
        {
            if (productReview == null)
                throw new ArgumentNullException("productReview");

			var store = _storeContext.CurrentStore;
			languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("Product.ProductReview", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddProductReviewTokens(tokens, productReview);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            // use customer email as reply address
			var replyTo = GetReplyToEmail(productReview.Customer);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
				replyTo.Item1, replyTo.Item2);
        }

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
		public virtual int SendQuantityBelowStoreOwnerNotification(Product product, int languageId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("QuantityBelow.StoreOwnerNotification", store.Id);
			if (messageTemplate == null)
                return 0;

			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddProductTokens(tokens, product, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "new VAT sumitted" notification to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vatName">Received VAT name</param>
        /// <param name="vatAddress">Received VAT address</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewVatSubmittedStoreOwnerNotification(Customer customer,
            string vatName, string vatAddress, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("NewVATSubmitted.StoreOwnerNotification", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, customer);
			tokens.Add(new Token("VatValidationResult.Name", vatName));
			tokens.Add(new Token("VatValidationResult.Address", vatAddress));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            // use customer email as reply address
			var replyTo = GetReplyToEmail(customer);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
				replyTo.Item1, replyTo.Item2);
        }

        /// <summary>
        /// Sends a blog comment notification message to a store owner
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendBlogCommentNotificationMessage(BlogComment blogComment, int languageId)
        {
            if (blogComment == null)
                throw new ArgumentNullException("blogComment");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("Blog.BlogComment", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddBlogCommentTokens(tokens, blogComment);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            // use customer email as reply address
			var replyTo = GetReplyToEmail(blogComment.Customer);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
				replyTo.Item1, replyTo.Item2);
        }

        /// <summary>
        /// Sends a news comment notification message to a store owner
        /// </summary>
        /// <param name="newsComment">News comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsCommentNotificationMessage(NewsComment newsComment, int languageId)
        {
            if (newsComment == null)
                throw new ArgumentNullException("newsComment");

			var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);
			
			var messageTemplate = GetActiveMessageTemplate("News.NewsComment", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddNewsCommentTokens(tokens, newsComment);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            // use customer email as sender/reply address
			var replyTo = GetReplyToEmail(newsComment.Customer);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
				replyTo.Item1, replyTo.Item2);
        }

        /// <summary>
        /// Sends a 'Back in stock' notification message to a customer
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendBackInStockNotification(BackInStockSubscription subscription, int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

			var store = _storeService.GetStoreById(subscription.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

			var messageTemplate = GetActiveMessageTemplate("Customer.BackInStock", store.Id);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, store);
			_messageTokenProvider.AddCustomerTokens(tokens, subscription.Customer);
			_messageTokenProvider.AddBackInStockTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var customer = subscription.Customer;
            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #endregion
    }
}
