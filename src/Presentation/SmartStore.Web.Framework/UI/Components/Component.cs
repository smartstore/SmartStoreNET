﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.IO;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI 
{
    public abstract class Component : IUiComponent
    {
        protected Component()
        {
            this.HtmlAttributes = new RouteValueDictionary();
			this.ComponentVersion = BootstrapVersion.V2;
        }

        public string Id
        {
            get
            {
                return !this.HtmlAttributes.ContainsKey("id") 
                    ? this.Name 
                    : (this.HtmlAttributes["id"] as string);
            }
            set
            {
                this.HtmlAttributes["id"] = value;
            }
        }

        public string Name
        {
            get;
            set;
        }

        public IDictionary<string, object> HtmlAttributes 
        {
            get;
            private set;
        }



        public virtual bool NameIsRequired
        {
            get 
            {
                return false;
            }
        }

		public BootstrapVersion ComponentVersion
		{
			get;
			set;
		}
	}

}
