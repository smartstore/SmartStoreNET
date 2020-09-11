using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Messages
{
    public partial class MessageTemplateService : IMessageTemplateService
    {
        private const string MESSAGETEMPLATES_ALL_KEY = "SmartStore.messagetemplate.all-{0}";
        private const string MESSAGETEMPLATES_BY_NAME_KEY = "SmartStore.messagetemplate.name-{0}-{1}";
        private const string MESSAGETEMPLATES_PATTERN_KEY = "SmartStore.messagetemplate.*";

        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly ILanguageService _languageService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IRequestCache _requestCache;

        public MessageTemplateService(
            IRequestCache requestCache,
            IRepository<StoreMapping> storeMappingRepository,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IStoreMappingService storeMappingService,
            IRepository<MessageTemplate> messageTemplateRepository)
        {
            _requestCache = requestCache;
            _storeMappingRepository = storeMappingRepository;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _storeMappingService = storeMappingService;
            _messageTemplateRepository = messageTemplateRepository;

            QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteMessageTemplate(MessageTemplate messageTemplate)
        {
            Guard.NotNull(messageTemplate, nameof(messageTemplate));

            _messageTemplateRepository.Delete(messageTemplate);

            _requestCache.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);
        }

        public virtual void InsertMessageTemplate(MessageTemplate messageTemplate)
        {
            Guard.NotNull(messageTemplate, nameof(messageTemplate));

            _messageTemplateRepository.Insert(messageTemplate);

            _requestCache.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);
        }

        public virtual void UpdateMessageTemplate(MessageTemplate messageTemplate)
        {
            Guard.NotNull(messageTemplate, nameof(messageTemplate));

            _messageTemplateRepository.Update(messageTemplate);

            _requestCache.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);
        }

        public virtual MessageTemplate GetMessageTemplateById(int messageTemplateId)
        {
            if (messageTemplateId == 0)
                return null;

            return _messageTemplateRepository.GetById(messageTemplateId);
        }

        public virtual MessageTemplate GetMessageTemplateByName(string messageTemplateName, int storeId)
        {
            Guard.NotEmpty(messageTemplateName, nameof(messageTemplateName));

            string key = string.Format(MESSAGETEMPLATES_BY_NAME_KEY, messageTemplateName, storeId);
            return _requestCache.Get(key, () =>
            {
                var query = _messageTemplateRepository.Table;
                query = query.Where(t => t.Name == messageTemplateName);
                query = query.OrderBy(t => t.Id);
                var templates = query.ToList();

                //store mapping
                if (storeId > 0)
                {
                    return templates.Where(t => _storeMappingService.Authorize(t, storeId)).FirstOrDefault();
                }

                return templates.FirstOrDefault();
            });

        }

        public virtual IList<MessageTemplate> GetAllMessageTemplates(int storeId)
        {
            string key = string.Format(MESSAGETEMPLATES_ALL_KEY, storeId);
            return _requestCache.Get(key, () =>
            {
                var query = _messageTemplateRepository.Table;
                query = query.OrderBy(t => t.Name);

                // Store mapping
                if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
                {
                    query = from t in query
                            join sm in _storeMappingRepository.Table
                            on new { c1 = t.Id, c2 = "MessageTemplate" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into t_sm
                            from sm in t_sm.DefaultIfEmpty()
                            where !t.LimitedToStores || storeId == sm.StoreId
                            select t;

                    // Only distinct items (group by ID)
                    query = from t in query
                            group t by t.Id into tGroup
                            orderby tGroup.Key
                            select tGroup.FirstOrDefault();
                    query = query.OrderBy(t => t.Name);
                }

                return query.ToList();
            });
        }

        public virtual MessageTemplate CopyMessageTemplate(MessageTemplate messageTemplate)
        {
            Guard.NotNull(messageTemplate, nameof(messageTemplate));

            var mtCopy = new MessageTemplate
            {
                Name = messageTemplate.Name,
                To = messageTemplate.To,
                ReplyTo = messageTemplate.ReplyTo,
                BccEmailAddresses = messageTemplate.BccEmailAddresses,
                Subject = messageTemplate.Subject,
                Body = messageTemplate.Body,
                IsActive = messageTemplate.IsActive,
                EmailAccountId = messageTemplate.EmailAccountId,
                LimitedToStores = messageTemplate.LimitedToStores
                // INFO: we do not copy attachments
            };

            InsertMessageTemplate(mtCopy);

            var languages = _languageService.GetAllLanguages(true);

            // localization
            foreach (var lang in languages)
            {
                string bccEmailAddresses = messageTemplate.GetLocalized(x => x.BccEmailAddresses, lang, false, false);
                if (bccEmailAddresses.HasValue())
                    _localizedEntityService.SaveLocalizedValue(mtCopy, x => x.BccEmailAddresses, bccEmailAddresses, lang.Id);

                string subject = messageTemplate.GetLocalized(x => x.Subject, lang, false, false);
                if (subject.HasValue())
                    _localizedEntityService.SaveLocalizedValue(mtCopy, x => x.Subject, subject, lang.Id);

                string body = messageTemplate.GetLocalized(x => x.Body, lang, false, false);
                if (body.HasValue())
                    _localizedEntityService.SaveLocalizedValue(mtCopy, x => x.Body, subject, lang.Id);

                int emailAccountId = messageTemplate.GetLocalized(x => x.EmailAccountId, lang, false, false);
                if (emailAccountId > 0)
                    _localizedEntityService.SaveLocalizedValue(mtCopy, x => x.EmailAccountId, emailAccountId, lang.Id);
            }

            // store mapping
            var selectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(messageTemplate);
            foreach (var id in selectedStoreIds)
            {
                _storeMappingService.InsertStoreMapping(mtCopy, id);
            }

            return mtCopy;
        }
    }
}
