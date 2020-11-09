using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Messages
{
    public partial class CampaignService : ICampaignService
    {
        private readonly ICommonServices _services;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;

        public CampaignService(
            ICommonServices services,
            IRepository<Campaign> campaignRepository,
            IMessageTemplateService messageTemplateService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreMappingService storeMappingService,
            IAclService aclService)
        {
            _services = services;
            _campaignRepository = campaignRepository;
            _messageTemplateService = messageTemplateService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;

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

        public virtual int SendCampaign(Campaign campaign)
        {
            Guard.NotNull(campaign, nameof(campaign));

            var totalEmailsSent = 0;
            var pageIndex = -1;
            int[] storeIds = null;
            int[] rolesIds = null;
            var alreadyProcessedEmails = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            if (campaign.LimitedToStores)
            {
                storeIds = _storeMappingService.GetStoreMappings(campaign)
                    .Select(x => x.StoreId)
                    .Distinct()
                    .ToArray();
            }

            if (campaign.SubjectToAcl)
            {
                rolesIds = _aclService.GetAclRecords(campaign)
                    .Select(x => x.CustomerRoleId)
                    .Distinct()
                    .ToArray();
            }

            while (true)
            {
                var subscribers = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(null, ++pageIndex, 500, false, storeIds, rolesIds);

                foreach (var subscriber in subscribers)
                {
                    // Create only one message per subscription email.
                    if (alreadyProcessedEmails.Contains(subscriber.Subscription.Email))
                    {
                        continue;
                    }

                    if (subscriber.Customer != null && !subscriber.Customer.Active)
                    {
                        continue;
                    }

                    var result = SendCampaign(campaign, subscriber);
                    if ((result?.Email?.Id ?? 0) != 0)
                    {
                        alreadyProcessedEmails.Add(subscriber.Subscription.Email);

                        ++totalEmailsSent;
                    }
                }

                if (!subscribers.HasNextPage)
                {
                    break;
                }
            }

            return totalEmailsSent;
        }

        public virtual CreateMessageResult SendCampaign(Campaign campaign, NewsletterSubscriber subscriber)
        {
            Guard.NotNull(campaign, nameof(campaign));

            if (subscriber?.Subscription == null)
            {
                return null;
            }

            var messageContext = new MessageContext
            {
                MessageTemplate = GetCampaignTemplate(),
                Customer = subscriber.Customer
            };

            var message = _services.MessageFactory.CreateMessage(messageContext, true, subscriber.Subscription, campaign);
            return message;
        }

        public virtual CreateMessageResult Preview(Campaign campaign)
        {
            Guard.NotNull(campaign, nameof(campaign));

            var messageContext = new MessageContext
            {
                MessageTemplate = GetCampaignTemplate(),
                TestMode = true
            };

            var subscription = _services.MessageFactory.GetTestModels(messageContext).OfType<NewsLetterSubscription>().FirstOrDefault();

            var message = _services.MessageFactory.CreateMessage(messageContext, false /* do NOT queue */, subscription, campaign);
            return message;
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
