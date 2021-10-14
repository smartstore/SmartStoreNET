using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace QTRADO.WMAddOn.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<QTRADO.WMAddOn.Data.WMAddOnObjectContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Data\Migrations";
            ContextKey = "QTRADO.WMAddOn"; // DO NOT CHANGE!
        }

        protected override void Seed(QTRADO.WMAddOn.Data.WMAddOnObjectContext context)
        {
        }
    }
}
