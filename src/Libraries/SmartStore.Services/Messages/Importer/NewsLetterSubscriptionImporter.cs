using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Logging;
using SmartStore.Services.DataExchange.Import;

namespace SmartStore.Services.Messages.Importer
{
	public class NewsLetterSubscriptionImporter : IEntityImporter
	{
		private readonly ICommonServices _services;
		private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;

		public NewsLetterSubscriptionImporter(
			ICommonServices services,
			IRepository<NewsLetterSubscription> subscriptionRepository)
		{
			_services = services;
			_subscriptionRepository = subscriptionRepository;
		}

		public void Execute(ImportExecuteContext context)
		{
			var utcNow = DateTime.UtcNow;
			var currentStoreId = _services.StoreContext.CurrentStore.Id;

			var toAdd = new List<NewsLetterSubscription>();
			var toUpdate = new List<NewsLetterSubscription>();

			using (var scope = new DbContextScope(ctx: _services.DbContext, autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
			{
				var segmenter = new ImportDataSegmenter<NewsLetterSubscription>(context.DataTable);
				segmenter.Culture = CultureInfo.CurrentCulture;

				while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
				{
					var batch = segmenter.CurrentBatch;

					_subscriptionRepository.Context.DetachAll(false);

					context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

					foreach (var row in batch)
					{
						string id = null;

						try
						{
							var email = row.GetDataValue<string>("Email");
							id = email;

							var active = row.GetDataValue<bool>("Active");
							var storeId = row.GetDataValue<int>("StoreId");

							if (storeId == 0)
								storeId = currentStoreId;

							if (email.IsEmpty())
							{
								context.Log.Warning("Skipped empty email address".FormatInvariant(email));
								continue;
							}

							if (email.Length > 255)
							{
								context.Log.Warning("Skipped email address '{0}' because it exceeds the maximum allowed length of 255".FormatInvariant(email));
								continue;
							}

							if (!email.IsEmail())
							{
								context.Log.Warning("Skipped invalid email address '{0}'".FormatInvariant(email));
								continue;
							}

							var subscription = _subscriptionRepository.Table
								.OrderBy(x => x.Id)
								.FirstOrDefault(x => x.Email == email && x.StoreId == storeId);

							if (subscription == null)
							{
								subscription = new NewsLetterSubscription
								{
									Active = active,
									CreatedOnUtc = utcNow,
									Email = email,
									NewsLetterSubscriptionGuid = Guid.NewGuid(),
									StoreId = storeId
								};

								toAdd.Add(subscription);
								++context.RecordsAdded;
							}
							else
							{
								subscription.Active = active;

								toUpdate.Add(subscription);
								++context.RecordsUpdated;
							}

							// insert new subscribers
							_subscriptionRepository.AutoCommitEnabled = true;
							_subscriptionRepository.InsertRange(toAdd, 500);
							toAdd.Clear();

							// update modified subscribers
							_subscriptionRepository.AutoCommitEnabled = null;
							toUpdate.Each(x => _subscriptionRepository.Update(x));
							_subscriptionRepository.Context.SaveChanges();
							toUpdate.Clear();
						}
						catch (Exception exception)
						{
							context.RecordException(exception, id.NaIfEmpty());
						}
					}
				}
			}
		}
	}
}
