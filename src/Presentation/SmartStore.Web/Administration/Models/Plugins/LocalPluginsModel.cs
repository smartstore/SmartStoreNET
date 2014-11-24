using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Collections;
using SmartStore.Admin.Models.Stores;

namespace SmartStore.Admin.Models.Plugins
{

    public class LocalPluginsModel : ModelBase
    {

        public LocalPluginsModel()
        {
            this.Groups = new Multimap<string, PluginModel>();
        }

		public List<StoreModel> AvailableStores { get; set; }

        public Multimap<string, PluginModel> Groups { get; set; }

        public ICollection<PluginModel> AllPlugins
        {
            get
            {
                return Groups.SelectMany(k => k.Value).ToList().AsReadOnly();
            }
        }

    }

}