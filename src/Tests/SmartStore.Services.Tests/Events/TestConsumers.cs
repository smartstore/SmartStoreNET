using System.Threading.Tasks;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Export.Events;
using SmartStore.Services.Media.Imaging;

namespace SmartStore.Services.Tests.Events
{
    public class TestConsumer1 : IConsumer
    {
        public void Handle(AppStartedEvent msg, ICacheManager s1, ICommonServices s2)
        {
        }

        public void HandleEvent(AppInitScheduledTasksEvent msg)
        {
        }

        public void Consume(ConsumeContext<AppRegisterGlobalFiltersEvent> msg)
        {
        }
    }

    public class TestConsumer2 : IConsumer
    {
        public Task HandleAsync(CategoryTreeChangedEvent msg)
        {
            return Task.FromResult(0);
        }

        [FireForget]
        public void HandleEvent(CustomerRegisteredEvent msg)
        {
        }

        public void Handle(ConsumeContext<AppStartedEvent> msg)
        {
        }

        public Task ConsumeAsync(AppInitScheduledTasksEvent msg)
        {
            return Task.FromResult(0);
        }
    }

    public class TestConsumer3 : IConsumer
    {
        public void Handle(CategoryTreeChangedEvent msg)
        {
        }

        [FireForget]
        public void HandleEvent(ConsumeContext<RowExportingEvent> msg)
        {
        }

        [FireForget]
        public Task HandleEventAsync(OrderPaidEvent msg)
        {
            return Task.FromResult(0);
        }

        public void NotAnEvent(AppStartedEvent msg)
        {
        }
    }

    public class HasAmbigousConsumers1 : IConsumer
    {
        public void Handle(AppStartedEvent msg)
        {
        }

        public void HandleEvent(ConsumeContext<AppStartedEvent> msg)
        {
        }
    }

    public class HasAmbigousConsumers2 : IConsumer
    {
        public void Handle(AppStartedEvent msg)
        {
        }

        public Task HandleAsync(AppStartedEvent msg)
        {
            return Task.FromResult(0);
        }
    }

    public class InvalidConsumer1 : IConsumer
    {
        public string Handle(AppStartedEvent msg)
        {
            return string.Empty;
        }
    }

    public class InvalidConsumer2 : IConsumer
    {
        public void ConsumeAsync(AppStartedEvent msg) { }
    }

    public class InvalidConsumer3 : IConsumer
    {
        public void Consume() { }
    }

    public class InvalidConsumer4 : IConsumer
    {
        public void HandleEvent(AppStartedEvent msg, bool something = true) { }
    }

    public class InvalidConsumer5 : IConsumer
    {
        public void HandleEvent(AppStartedEvent msg, out bool something) { something = false; }
    }

    public class InvalidConsumer6 : IConsumer
    {
        public void HandleEvent(AppStartedEvent msg, ref CachedImage result) { }
    }
}
