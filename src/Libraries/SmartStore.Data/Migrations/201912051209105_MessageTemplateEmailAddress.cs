namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Messages;
    using SmartStore.Data.Setup;

    public partial class MessageTemplateEmailAddress : DbMigration, IDataSeeder<SmartObjectContext>
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
            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false))
            {
                var gdprAdminEmail = context.Set<MessageTemplate>().Where(x => x.Name.Equals("Admin.AnonymizeRequest")).FirstOrDefault();
                if (gdprAdminEmail != null && gdprAdminEmail.To.Equals("{{ Customer.FullName }} &lt;{{ Customer.Email }}&gt;"))
                {
                    gdprAdminEmail.To = "{{ Email.DisplayName }} &lt;{{ Email.Email }}&gt;";
                }

                context.SaveChanges();
            }
        }
    }
}
