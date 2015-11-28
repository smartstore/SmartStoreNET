using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;

namespace SmartStore.Web.Framework.UI {

    public class Button : IHtmlString {

        TagBuilder _builder;

        public Button(string tagName = "a", string text = "Mein Text") {
            _builder = new TagBuilder(tagName);
            this.Text = text;
        }

        public string Text { get; set; }

        public virtual string ToHtmlString() {
            _builder.AddCssClass("btn");
            _builder.InnerHtml = this.Text;
            _builder.MergeAttribute("href", "#");
            return _builder.ToString(TagRenderMode.Normal);
        }

        public override string ToString() {
            return this.ToHtmlString();
        }

    }

}
