using SmartStore.Core.Plugins;
using SmartStore.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace Strube.Export
{
    public class Plugin:BasePlugin
    {
        private readonly ICommonServices _services;

        public Plugin(ICommonServices services)
        {
            this._services = services;
        }

        public static string SystemName
        {
            get { return "Strube.Export"; }
        }

        /// <summary>
        /// Install Plugin
        /// </summary>
        public override void Install()
        {
            var settings = _services.Settings;
            var loc = _services.Localization;

            // add resources
            loc.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        /// <summary>
        /// Uninstall Plugin
        /// </summary>
        public override void Uninstall()
        {
            var settings = _services.Settings;
            var loc = _services.Localization;

            // delete resources
            loc.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            //loc.DeleteLocaleStringResources("Plugins.Tax.CountryStateZip");
            //loc.DeleteLocaleStringResources("Plugins.Tax.FixedRate");

            //var migrator = new DbMigrator(new Configuration());
            //migrator.Update(DbMigrator.InitialDatabase);

            base.Uninstall();
        }

    }
}