using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;
using SmartStore.Services.Customers;
using SmartStore.Core;
using SmartStore.Core.Email;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Messages
{
    public partial class CampaignService : ICampaignService
    {
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IEmailSender _emailSender;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly ITokenizer _tokenizer;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ICustomerService _customerService;
		private readonly IStoreContext _storeContext;
		private readonly IStoreMappingService _storeMappingService;
        private readonly IEventPublisher _eventPublisher;

        public CampaignService(IRepository<Campaign> campaignRepository,
            IEmailSender emailSender, IMessageTokenProvider messageTokenProvider,
            ITokenizer tokenizer, IQueuedEmailService queuedEmailService,
			ICustomerService customerService,
			IStoreContext storeContext, 
			IStoreMappingService storeMappingService,
			IEventPublisher eventPublisher)
        {
            this._campaignRepository = campaignRepository;
            this._emailSender = emailSender;
            this._messageTokenProvider = messageTokenProvider;
            this._tokenizer = tokenizer;
            this._queuedEmailService = queuedEmailService;
            this._customerService = customerService;
			this._storeContext = storeContext;
			this._storeMappingService = storeMappingService;
            this._eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Inserts a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        public virtual void InsertCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            _campaignRepository.Insert(campaign);

            //event notification
            _eventPublisher.EntityInserted(campaign);
        }

        /// <summary>
        /// Updates a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public virtual void UpdateCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            _campaignRepository.Update(campaign);

            //event notification
            _eventPublisher.EntityUpdated(campaign);
        }

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public virtual void DeleteCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            _campaignRepository.Delete(campaign);

            //event notification
            _eventPublisher.EntityDeleted(campaign);
        }

        /// <summary>
        /// Gets a campaign by identifier
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        /// <returns>Campaign</returns>
        public virtual Campaign GetCampaignById(int campaignId)
        {
            if (campaignId == 0)
                return null;

            var campaign = _campaignRepository.GetById(campaignId);
            return campaign;

        }

        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <returns>Campaign collection</returns>
        public virtual IList<Campaign> GetAllCampaigns()
        {
            var query = from c in _campaignRepository.Table
                        orderby c.CreatedOnUtc
                        select c;

            var campaigns = query.ToList();

            return campaigns;
        }
        
        /// <summary>
        /// Sends a campaign to specified emails
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Total emails sent</returns>
        public virtual int SendCampaign(Campaign campaign, EmailAccount emailAccount, IEnumerable<NewsLetterSubscription> subscriptions)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

			if (subscriptions == null || subscriptions.Count() <= 0)
				return 0;

            int totalEmailsSent = 0;

			var subscriptionData = subscriptions
				.Where(x => _storeMappingService.Authorize<Campaign>(campaign, x.StoreId))
				.GroupBy(x => x.Email);

			foreach (var group in subscriptionData)
            {
				var subscription = group.First();	// only one email per email address

                var tokens = new List<Token>();
				_messageTokenProvider.AddStoreTokens(tokens, _storeContext.CurrentStore);
                _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

                var customer = _customerService.GetCustomerByEmail(subscription.Email);
                if (customer != null)
                    _messageTokenProvider.AddCustomerTokens(tokens, customer);

                string subject = _tokenizer.Replace(campaign.Subject, tokens, false);
                string body = _tokenizer.Replace(campaign.Body, tokens, true);

                var email = new QueuedEmail()
                {
                    Priority = 3,
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = subscription.Email,
                    Subject = subject,
                    Body = body,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                };

                _queuedEmailService.InsertQueuedEmail(email);
                totalEmailsSent++;
            }
            return totalEmailsSent;
        }

        /// <summary>
        /// Sends a campaign to specified email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="email">Email</param>
        public virtual void SendCampaign(Campaign campaign, EmailAccount emailAccount, string email)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            var tokens = new List<Token>();
			_messageTokenProvider.AddStoreTokens(tokens, _storeContext.CurrentStore);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, new NewsLetterSubscription() {
                Email = email
            });

            var customer = _customerService.GetCustomerByEmail(email);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);
            
            string subject = _tokenizer.Replace(campaign.Subject, tokens, false);
            string body = _tokenizer.Replace(campaign.Body, tokens, true);

			var to = new EmailAddress(email);
            var from = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

			var msg = new EmailMessage(to, subject, body, from);

            _emailSender.SendEmail(new SmtpContext(emailAccount), msg);
        }
    }
}
