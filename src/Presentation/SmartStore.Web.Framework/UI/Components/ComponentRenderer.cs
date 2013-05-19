using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Web.UI;
using System.IO;

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
            Guard.NotNull(() => this.Component);
            if (this.Component.NameIsRequired && !this.Component.Id.HasValue())
            {
                throw Error.InvalidOperation("A component must have a unique 'Name'. Please provide a name.");
            }
        }

        protected void WriteHtml(HtmlTextWriter writer)
        {
            this.VerifyState();
            this.Component.Id = SanitizeId(this.Component.Id);

            this.WriteHtmlCore(writer);
        }

        protected virtual void WriteHtmlCore(HtmlTextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
            using (HtmlTextWriter htmlTextWriter = new HtmlTextWriter(this.ViewContext.Writer))
            {
                this.WriteHtml(htmlTextWriter);
            }
        }

        public string ToHtmlString()
        {
            string str;
            using (StringWriter stringWriter = new StringWriter())
            {
                this.WriteHtml(new HtmlTextWriter(stringWriter));
                str = stringWriter.ToString();
            }
            return str;
        }


        protected string SanitizeId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(id.Length);
            int index = id.IndexOf("#");
            int num2 = id.LastIndexOf("#");
            if (num2 > index)
            {
                ReplaceInvalidCharacters(id.Substring(0, index), builder);
                builder.Append(id.Substring(index, (num2 - index) + 1));
                ReplaceInvalidCharacters(id.Substring(num2 + 1), builder);
            }
            else
            {
                ReplaceInvalidCharacters(id, builder);
            }
            return builder.ToString();
        }

        private static bool IsValidCharacter(char c)
        {
            return (((c != '?') && (c != '!')) && ((c != '#') && (c != '.')));
        }

        private static void ReplaceInvalidCharacters(string part, StringBuilder builder)
        {
            for (int i = 0; i < part.Length; i++)
            {
                char c = part[i];
                if (IsValidCharacter(c))
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append(HtmlHelper.IdAttributeDotReplacement);
                }
            }
        }

    }

}
