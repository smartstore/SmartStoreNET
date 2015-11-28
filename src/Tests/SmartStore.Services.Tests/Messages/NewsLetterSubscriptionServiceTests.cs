using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;
using SmartStore.Services.Messages;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Tests.Messages
{
    [TestFixture]
	public class NewsLetterSubscriptionServiceTests : ServiceTest
    {
		IEventPublisher _eventPublisher;
		IRepository<NewsLetterSubscription> _subscriptionRepository;
		IDbContext _dbContext;
		IStoreService _storeService;
		NewsLetterSubscriptionService _newsLetterSubscriptionService;

		[SetUp]
		public new void SetUp()
		{
			_eventPublisher = MockRepository.GenerateStub<IEventPublisher>();
			_subscriptionRepository = MockRepository.GenerateStub<IRepository<NewsLetterSubscription>>();
			_dbContext = MockRepository.GenerateStub<IDbContext>();
			_storeService = MockRepository.GenerateStub<IStoreService>();

			_newsLetterSubscriptionService = new NewsLetterSubscriptionService(_dbContext, _subscriptionRepository, _eventPublisher, _storeService);
		}

        /// <summary>
        /// Verifies the active insert triggers subscribe event.
        /// </summary>
        [Test]
        public void VerifyActiveInsertTriggersSubscribeEvent()
        {
            var subscription = new NewsLetterSubscription { Active = true, Email = "skyler@csharpwebdeveloper.com", StoreId = 1 };

			_newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription, true);

            _eventPublisher.AssertWasCalled(x => x.Publish(new EmailSubscribedEvent(subscription.Email)));
        }

        /// <summary>
        /// Verifies the delete triggers unsubscribe event.
        /// </summary>
        [Test]
        public void VerifyDeleteTriggersUnsubscribeEvent()
        {
			var subscription = new NewsLetterSubscription { Active = true, Email = "skyler@csharpwebdeveloper.com", StoreId = 1 };

			_newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription, true);

            _eventPublisher.AssertWasCalled(x => x.Publish(new EmailUnsubscribedEvent(subscription.Email)));
        }

        /// <summary>
        /// Verifies the email update triggers unsubscribe and subscribe event.
        /// </summary>
        [Test]
        [Ignore("Ignoring until a solution to the IDbContext methods are found. -SRS")]
        public void VerifyEmailUpdateTriggersUnsubscribeAndSubscribeEvent()
        {
            //Prepare the original result
			var originalSubscription = new NewsLetterSubscription { Active = true, Email = "skyler@csharpwebdeveloper.com", StoreId = 1 };
			_subscriptionRepository.Stub(m => m.GetById(Arg<object>.Is.Anything)).Return(originalSubscription);

			var subscription = new NewsLetterSubscription { Active = true, Email = "skyler@tetragensoftware.com", StoreId = 1 };

			_newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription, true);

            _eventPublisher.AssertWasCalled(x => x.Publish(new EmailUnsubscribedEvent(originalSubscription.Email)));
            _eventPublisher.AssertWasCalled(x => x.Publish(new EmailSubscribedEvent(subscription.Email)));
        }

        /// <summary>
        /// Verifies the inactive to active update triggers subscribe event.
        /// </summary>
        [Test]
        [Ignore("Ignoring until a solution to the IDbContext methods are found. -SRS")]
        public void VerifyInactiveToActiveUpdateTriggersSubscribeEvent()
        {
            //Prepare the original result
			var originalSubscription = new NewsLetterSubscription { Active = false, Email = "skyler@csharpwebdeveloper.com", StoreId = 1 };
			_subscriptionRepository.Stub(m => m.GetById(Arg<object>.Is.Anything)).Return(originalSubscription);

			var subscription = new NewsLetterSubscription { Active = true, Email = "skyler@csharpwebdeveloper.com", StoreId = 1 };

            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription, true);

            _eventPublisher.AssertWasCalled(x => x.Publish(new EmailSubscribedEvent(subscription.Email)));
        }

        /// <summary>
        /// Verifies the insert event is fired.
        /// </summary>
        [Test]
        public void VerifyInsertEventIsFired()
        {
			_newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription { Email = "skyler@csharpwebdeveloper.com", StoreId = 1 });

            _eventPublisher.AssertWasCalled(x => x.EntityInserted(Arg<NewsLetterSubscription>.Is.Anything));
        }

        /// <summary>
        /// Verifies the update event is fired.
        /// </summary>
        [Test]
        [Ignore("Ignoring until a solution to the IDbContext methods are found. -SRS")]
        public void VerifyUpdateEventIsFired()
        {
            //Prepare the original result
			var originalSubscription = new NewsLetterSubscription { Active = false, Email = "skyler@csharpwebdeveloper.com", StoreId = 1 };
			_subscriptionRepository.Stub(m => m.GetById(Arg<object>.Is.Anything)).Return(originalSubscription);

			_newsLetterSubscriptionService.UpdateNewsLetterSubscription(new NewsLetterSubscription { Email = "skyler@csharpwebdeveloper.com", StoreId = 1 });

            _eventPublisher.AssertWasCalled(x => x.EntityUpdated(Arg<NewsLetterSubscription>.Is.Anything));
        }
    }
}