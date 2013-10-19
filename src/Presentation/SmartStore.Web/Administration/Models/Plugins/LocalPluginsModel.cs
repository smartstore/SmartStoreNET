using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Collections;

// codehint: sm-add (whole file)

namespace SmartStore.Admin.Models.Plugins
{

    public class LocalPluginsModel : ModelBase
    {

        public LocalPluginsModel()
        {
            this.Groups = new Multimap<string, PluginModel>();
        }

        /// <summary>
        /// Tuple-1: group name, Tuple-2: plugins count
        /// </summary>
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