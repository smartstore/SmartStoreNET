using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Data;
using SmartStore.Core.Data;

namespace SmartStore.Web.Framework.Controllers
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class UnitOfWorkAttribute : ActionFilterAttribute
    {

        public UnitOfWorkAttribute() 
            : this(null, Int32.MaxValue)
        {
        }

        public UnitOfWorkAttribute(int ordinal) 
            : this(null, ordinal)
        {
        }

        public UnitOfWorkAttribute(string alias)
            : this(alias, Int32.MaxValue)
        {
        }

        public UnitOfWorkAttribute(string alias, int ordinal)
        {
            this.Alias = alias.NullEmpty();
            base.Order = ordinal;
        }

		public Func<string, IDbContext> DbContext { get; set; }

        public string Alias { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
                return;

			var context = DbContext(this.Alias);
            
            if (context != null && context.HasChanges)
            {
                try
                {
                    context.SaveChanges();
                }
                catch /*(Exception ex)*/
                {
                    // do exactly WHAT now?
                }
            }
        }
    }

}
