using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Cms;
using SmartStore.Data.Migrations;
using SmartStore.Data.Setup;

namespace SmartStore.Data
{
    public class SmartObjectContext : ObjectContextBase
    {
        static SmartObjectContext()
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            var initializer = new MigrateDatabaseInitializer<SmartObjectContext, MigrationsConfiguration>
            {
                TablesToCheck = new[] { "Customer", "Discount", "Order", "Product", "ShoppingCartItem", "QueuedEmailAttachment", "ExportProfile" }
            };
            Database.SetInitializer(initializer);
        }

        /// <summary>
        /// For tooling support, e.g. EF Migrations
        /// </summary>
        public SmartObjectContext()
            : base()
        {
        }

        public SmartObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ////dynamically load all configuration
            ////System.Type configType = typeof(LanguageMap);   //any of your configuration classes here
            ////var typesToRegister = Assembly.GetAssembly(configType).GetTypes()
            //var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
            //    .Where(type => 
            //        !String.IsNullOrEmpty(type.Namespace)
            //        && type.BaseType != null 
            //        && type.BaseType.IsGenericType 
            //        && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            var typesToRegister = from t in Assembly.GetExecutingAssembly().GetTypes()
                                  where t.Namespace.HasValue() &&
                                        t.BaseType != null &&
                                        t.BaseType.IsGenericType
                                  let genericType = t.BaseType.GetGenericTypeDefinition()
                                  where genericType == typeof(EntityTypeConfiguration<>) || genericType == typeof(ComplexTypeConfiguration<>)
                                  select t;

            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }
            //...or do it manually below. For example,
            //modelBuilder.Configurations.Add(new LanguageMap());

            modelBuilder.Entity<MenuRecord>();
            modelBuilder.Entity<MenuItemRecord>();

            base.OnModelCreating(modelBuilder);
        }

    }
}