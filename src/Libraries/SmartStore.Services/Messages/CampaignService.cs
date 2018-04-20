using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Customers;
using SmartStore.Core.Email;
using SmartStore.Services.Stores;
using SmartStore.Templating;
using SmartStore.Core.Localization;

namespace SmartStore.Services.Messages
{
    public partial class CampaignService : ICampaignService
    {
		private readonly ICommonServices _services;
		private readonly IRepository<Campaign> _campaignRepository;
		private readonly IMessageTemplateService _messageTemplateService;
		private readonly IEmailSender _emailSender;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ICustomerService _customerService;
		private readonly IStoreMappingService _storeMappingService;

        public CampaignService(
			ICommonServices services,
			IRepository<Campaign> campaignRepository,
			IMessageTemplateService messageTemplateService,
			IEmailSender emailSender, 
			IQueuedEmailService queuedEmailService,
			ICustomerService customerService,
			IStoreMappingService storeMappingService)
        {
			_services = services;
			_campaignRepository = campaignRepository;
			_messageTemplateService = messageTemplateService;
            _emailSender = emailSender;
            _queuedEmailService = queuedEmailService;
            _customerService = customerService;
			_storeMappingService = storeMappingService;

			T = NullLocalizer.Instance;
        }

		public Localizer T { get; set; }
      
        public virtual void InsertCampaign(Campaign campaign)
        {
			Guard.NotNull(campaign, nameof(campaign));

            _campaignRepository.Insert(campaign);
        }

        public virtual void UpdateCampaign(Campaign campaign)
        {
			Guard.NotNull(campaign, nameof(campaign));

			_campaignRepository.Update(campaign);
        }

        public virtual void DeleteCampaign(Campaign campaign)
        {
			Guard.NotNull(campaign, nameof(campaign));

			_campaignRepository.Delete(campaign);
        }

        public virtual Campaign GetCampaignById(int campaignId)
        {
            if (campaignId == 0)
                return null;

            var campaign = _campaignRepository.GetById(campaignId);
            return campaign;

        }

        public virtual IList<Campaign> GetAllCampaigns()
        {
            var query = from c in _campaignRepository.Table
                        orderby c.CreatedOnUtc
                        select c;

            var campaigns = query.ToList();

            return campaigns;
        }

        public virtual int SendCampaign(Campaign campaign, IEnumerable<NewsLetterSubscription> subscriptions)
        {
			Guard.NotNull(campaign, nameof(campaign));

			if (subscriptions == null || subscriptions.Count() <= 0)
				return 0;

            int totalEmailsQueued = 0;

			var subscriptionData = subscriptions
				.Where(x => _storeMappingService.Authorize<Campaign>(campaign, x.StoreId))
				.GroupBy(x => x.Email);

			foreach (var group in subscriptionData)
            {
				var subscription = group.First();   // only one email per email address
				var customer = _customerService.GetCustomerByEmail(subscription.Email);
				var messageContext = new MessageContext
				{
					MessageTemplate = GetCampaignTemplate(),
					Customer = customer
				};

				var msg = _services.MessageFactory.CreateMessage(messageContext, true, subscription, campaign);

				if (msg.Email?.Id != null)
				{
					totalEmailsQueued++;
				}
			}

            return totalEmailsQueued;
        }

		public virtual CreateMessageResult Preview(Campaign campaign)
		{
			Guard.NotNull(campaign, nameof(campaign));

			var messageContext = new MessageContext
			{
				MessageTemplate = GetCampaignTemplate(),
				TestMode = true
			};

			var subscription =_services.MessageFactory.GetTestModels(messageContext).OfType<NewsLetterSubscription>().FirstOrDefault();

			var result = _services.MessageFactory.CreateMessage(messageContext, false /* do NOT queue */, subscription, campaign);

			return result;
		}

		private MessageTemplate GetCampaignTemplate()
		{
			var messageTemplate = _messageTemplateService.GetMessageTemplateByName(MessageTemplateNames.SystemCampaign, _services.StoreContext.CurrentStore.Id);
			if (messageTemplate == null)
				throw new SmartException(T("Common.Error.NoMessageTemplate", MessageTemplateNames.SystemCampaign));

			return messageTemplate;
		}
	}
}
