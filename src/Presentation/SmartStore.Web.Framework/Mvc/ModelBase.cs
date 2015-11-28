using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using AutoMapper;

namespace SmartStore.Web.Framework.Mvc
{
	
	public sealed class CustomPropertiesDictionary : Dictionary<string, object>
	{
	}
	

	public abstract partial class ModelBase
    {
        public ModelBase()
        {
			this.CustomProperties = new CustomPropertiesDictionary();
        }
        
        public virtual void BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
        }

        /// <summary>
        /// Use this property to store any custom value for your models. 
        /// </summary>
		[IgnoreMap]
		public CustomPropertiesDictionary CustomProperties { get; set; }
    }


    public abstract partial class EntityModelBase : ModelBase
    {
        [SmartResourceDisplayName("Admin.Common.Entity.Fields.Id")]
        public virtual int Id { get; set; }
    }


	public abstract partial class TabbableModel : EntityModelBase
	{
		[IgnoreMap]
		public virtual string[] LoadedTabs { get; set; }
	}

}
