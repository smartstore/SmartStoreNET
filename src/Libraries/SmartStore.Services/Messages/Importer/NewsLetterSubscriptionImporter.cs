using System;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.DataExchange.Import.Events;

namespace SmartStore.Services.Messages.Importer
{
    public class NewsLetterSubscriptionImporter : IEntityImporter
    {
        private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;

        public NewsLetterSubscriptionImporter(IRepository<NewsLetterSubscription> subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public static string[] SupportedKeyFields => new string[] { "Email" };

        public static string[] DefaultKeyFields => new string[] { "Email" };

        public void Execute(ImportExecuteContext context)
        {
            var utcNow = DateTime.UtcNow;
            var currentStoreId = context.Services.StoreContext.CurrentStore.Id;

            using (var scope = new DbContextScope(ctx: context.Services.DbContext, hooksEnabled: false, autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
            {
                var segmenter = context.DataSegmenter;

                context.Result.TotalRecords = segmenter.TotalRows;

                while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
                {
                    var batch = segmenter.GetCurrentBatch<NewsLetterSubscription>();

                    // Perf: detach all entities
                    _subscriptionRepository.Context.DetachEntities<NewsLetterSubscription>(false);

                    context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

                    foreach (var row in batch)
                    {
                        try
                        {
                            var active = true;
                            var email = row.GetDataValue<string>("Email");
                            var storeId = row.GetDataValue<int>("StoreId");

                            if (storeId == 0)
                            {
                                storeId = currentStoreId;
                            }

                            if (row.HasDataValue("Active") && row.TryGetDataValue("Active", out active))
                            {
                            }
                            else
                            {
                                active = true;  // default
                            }

                            if (email.IsEmpty())
                            {
                                context.Result.AddWarning("Skipped empty email address", row.GetRowInfo(), "Email");
                                continue;
                            }

                            if (email.Length > 255)
                            {
                                context.Result.AddWarning("Skipped email address '{0}'. It exceeds the maximum allowed length of 255".FormatInvariant(email), row.GetRowInfo(), "Email");
                                continue;
                            }

                            if (!email.IsEmail())
                            {
                                context.Result.AddWarning("Skipped invalid email address '{0}'".FormatInvariant(email), row.GetRowInfo(), "Email");
                                continue;
                            }

                            NewsLetterSubscription subscription = null;

                            foreach (var keyName in context.KeyFieldNames)
                            {
                                switch (keyName)
                                {
                                    case "Email":
                                        subscription = _subscriptionRepository.Table
                                            .OrderBy(x => x.Id)
                                            .FirstOrDefault(x => x.Email == email && x.StoreId == storeId);
                                        break;
                                }

                                if (subscription != null)
                                    break;
                            }

                            if (subscription == null)
                            {
                                if (context.UpdateOnly)
                                {
                                    ++context.Result.SkippedRecords;
                                    continue;
                                }

                                subscription = new NewsLetterSubscription
                                {
                                    Active = active,
                                    CreatedOnUtc = utcNow,
                                    Email = email,
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    StoreId = storeId
                                };

                                _subscriptionRepository.Insert(subscription);
                                context.Result.NewRecords++;
                            }
                            else
                            {
                                subscription.Active = active;

                                _subscriptionRepository.Update(subscription);
                                context.Result.ModifiedRecords++;
                            }
                        }
                        catch (Exception exception)
                        {
                            context.Result.AddError(exception.ToAllMessages(), row.GetRowInfo());
                        }
                    } // for

                    _subscriptionRepository.Context.SaveChanges();

                    context.Services.EventPublisher.Publish(new ImportBatchExecutedEvent<NewsLetterSubscription>(context, batch));
                } // while
            }
        }
    }
}
