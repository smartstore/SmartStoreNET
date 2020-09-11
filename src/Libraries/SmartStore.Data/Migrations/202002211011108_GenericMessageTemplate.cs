namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Messages;
    using SmartStore.Data.Setup;

    public partial class GenericMessageTemplate : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            if (!DataSettings.DatabaseIsInstalled())
            {
                return;
            }

            context.MigrateLocaleResources(MigrateLocaleResources);

            var defaultEmailAccount = context.Set<EmailAccount>().FirstOrDefault(x => x.Email != null);
            var table = context.Set<MessageTemplate>();

            table.Add(new MessageTemplate
            {
                IsActive = true,
                Name = "System.Generic",
                To = "{{ Generic.Email }}",
                ReplyTo = "{{ Generic.ReplyTo }}",
                Subject = "{{ Generic.Subject }}",
                EmailAccountId = (defaultEmailAccount?.Id).GetValueOrDefault(),
                Body = @"{% extends 'master' %}
                    {% block 'body' %}
                    {{ Generic.Body }}
                    {% endblock %}"
            });

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Customers.Customers.SendEmail.EmailNotValid",
                "No valid e-mail address is stored for the customer.",
                "Für den Kunden ist keine gültige E-Mail-Adresse hinterlegt.");

            builder.Delete("Admin.Customers.Customers.SendPM.Message.Hint");
            builder.Delete("Admin.Customers.Customers.SendPM.Subject.Hint");
        }
    }
}
