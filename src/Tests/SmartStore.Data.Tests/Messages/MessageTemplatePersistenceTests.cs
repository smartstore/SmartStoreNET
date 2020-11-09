using NUnit.Framework;
using SmartStore.Core.Domain.Messages;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Messages
{
    [TestFixture]
    public class MessageTemplatePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_messageTemplate()
        {
            var mt = new MessageTemplate
            {
                Name = "Template1",
                To = "{{ Message.To }}",
                BccEmailAddresses = "Bcc",
                Subject = "Subj",
                Body = "Some text",
                IsActive = true,
                EmailAccountId = 1,
                LimitedToStores = true
            };


            var fromDb = SaveAndLoadEntity(mt);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Template1");
            fromDb.To.ShouldEqual("{{ Message.To }}");
            fromDb.BccEmailAddresses.ShouldEqual("Bcc");
            fromDb.Subject.ShouldEqual("Subj");
            fromDb.Body.ShouldEqual("Some text");
            fromDb.IsActive.ShouldBeTrue();
            fromDb.LimitedToStores.ShouldBeTrue();
            fromDb.EmailAccountId.ShouldEqual(1);
        }
    }
}