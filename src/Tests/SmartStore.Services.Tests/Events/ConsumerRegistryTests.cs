using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Export.Events;
using SmartStore.Services.Events;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Events
{
    [TestFixture]
    public class ConsumerRegistryTests
    {
        //private IEnumerable<Lazy<IConsumer, EventConsumerMetadata>> _validConsumers;

        [Test]
        public void Can_create_descriptors()
        {
            var validConsumers = new List<Lazy<IConsumer, EventConsumerMetadata>>
            {
                CreateConsumerRegistration(new TestConsumer1()),
                CreateConsumerRegistration(new TestConsumer2()),
                CreateConsumerRegistration(new TestConsumer3())
            };

            var registry = new ConsumerRegistry(validConsumers);

            var descriptors = registry.GetConsumers(new AppStartedEvent());
            descriptors.Count().ShouldEqual(2, "AppStartedEvent count");

            descriptors = registry.GetConsumers(new AppInitScheduledTasksEvent());
            descriptors.Count().ShouldEqual(2, "AppInitScheduledTasksEvent count");

            descriptors = registry.GetConsumers(new CustomerRegisteredEvent());
            descriptors.Count().ShouldEqual(1);
            var descriptor = descriptors.First();
            descriptor.ContainerType.ShouldEqual(typeof(TestConsumer2));
            descriptor.FireForget.ShouldEqual(true);
            descriptor.IsAsync.ShouldEqual(false);
            descriptor.WithEnvelope.ShouldEqual(false);

            descriptors = registry.GetConsumers(new OrderPaidEvent(null));
            descriptors.Count().ShouldEqual(1);
            descriptor = descriptors.First();
            descriptor.ContainerType.ShouldEqual(typeof(TestConsumer3));
            descriptor.FireForget.ShouldEqual(true);
            descriptor.IsAsync.ShouldEqual(true);

            descriptors = registry.GetConsumers(new RowExportingEvent());
            descriptors.Count().ShouldEqual(1);
            descriptor = descriptors.First();
            descriptor.ContainerType.ShouldEqual(typeof(TestConsumer3));
            descriptor.FireForget.ShouldEqual(true);
            descriptor.IsAsync.ShouldEqual(false);
            descriptor.WithEnvelope.ShouldEqual(true);
        }

        [Test]
        public void Finds_ambigous_consumers()
        {
            var consumers = new List<Lazy<IConsumer, EventConsumerMetadata>>
            {
                CreateConsumerRegistration(new HasAmbigousConsumers1())
            };
            typeof(AmbigousConsumerException).ShouldBeThrownBy(() => new ConsumerRegistry(consumers));

            consumers = new List<Lazy<IConsumer, EventConsumerMetadata>>
            {
                CreateConsumerRegistration(new HasAmbigousConsumers2())
            };
            typeof(AmbigousConsumerException).ShouldBeThrownBy(() => new ConsumerRegistry(consumers));
        }

        [Test]
        public void Finds_invalid_consumers()
        {
            foreach (var c in new IConsumer[]
            {
                new InvalidConsumer1(),
                new InvalidConsumer2(),
                new InvalidConsumer3(),
                new InvalidConsumer4(),
                new InvalidConsumer5(),
                new InvalidConsumer6()
            })
            {
                var consumers = new List<Lazy<IConsumer, EventConsumerMetadata>> { CreateConsumerRegistration(c) };
                typeof(NotSupportedException).ShouldBeThrownBy(() => new ConsumerRegistry(consumers));
            }
        }

        private Lazy<IConsumer, EventConsumerMetadata> CreateConsumerRegistration(IConsumer consumer, bool isActive = true)
        {
            var metadata = new EventConsumerMetadata
            {
                IsActive = isActive,
                ContainerType = consumer.GetType()
            };

            return new Lazy<IConsumer, EventConsumerMetadata>(() => consumer, metadata);
        }
    }
}
