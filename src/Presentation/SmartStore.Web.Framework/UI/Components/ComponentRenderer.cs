using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SmartStore.Web.Framework.UI
{
    public abstract class ComponentRenderer<TComponent> : IHtmlString where TComponent : Component
    {
        protected ComponentRenderer()
        {
        }

        protected ComponentRenderer(TComponent component)
        {
            this.Component = component;
        }

        protected internal TComponent Component
        {
            get;
            set;
        }

        protected internal HtmlHelper HtmlHelper
        {
            get;
            internal set;
        }

        protected internal ViewContext ViewContext
        {
            get;
            internal set;
        }

        protected internal ViewDataDictionary ViewData
        {
            get;
            internal set;
        }

        public virtual void VerifyState()
        {
            Guard.NotNull(this.Component, nameof(this.Component));

            if (this.Component.NameIsRequired && !this.Component.Id.HasValue())
            {
                throw Error.InvalidOperation("A component must have a unique 'Name'. Please provide a name.");
            }
        }

        protected string SanitizeId(string id)
        {
            return id.SanitizeHtmlId();
        }

        public virtual void Render()
        {
            WriteHtml(ViewContext.Writer);
        }

        public virtual string ToHtmlString()
        {
            using (var stringWriter = new StringWriter())
            {
                WriteHtml(stringWriter);
                var str = stringWriter.ToString();
                return str;
            }
        }

        protected void WriteHtml(TextWriter writer)
        {
            this.VerifyState();
            this.Component.Id = SanitizeId(this.Component.Id);

            using (var htmlWriter = new HtmlTextWriter(writer))
            {
                WriteHtmlCore(htmlWriter);
            }
        }

        protected virtual void WriteHtmlCore(HtmlTextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
