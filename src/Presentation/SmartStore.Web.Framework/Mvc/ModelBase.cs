using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Mvc
{
    public abstract partial class ModelBase
    {
        public ModelBase()
        {
            this.CustomProperties = new Dictionary<string, object>();
        }
        
        public virtual void BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
        }

        /// <summary>
        /// Use this property to store any custom value for your models. 
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; }
    }

    public abstract partial class EntityModelBase : ModelBase
    {
        [SmartResourceDisplayName("Admin.Common.Entity.Fields.Id")]
        public virtual int Id { get; set; }
    }
}
