using System.Collections.Generic;
using DotLiquid;

namespace SmartStore.Templating
{
    /// <summary>
    /// Published when a template zone is about to be rendered.
    /// By subscribing to this event, implementors can inject custom
    /// content to specific template zones.
    /// </summary>
    public sealed class ZoneRenderingEvent
    {
        private IList<Snippet> _snippets;

        public ZoneRenderingEvent(string zoneName, IDictionary<string, object> model)
        {
            ZoneName = zoneName;
            Model = model;
        }

        internal Context LiquidContext { get; set; }

        /// <summary>
        /// The name of the rendered template.
        /// </summary>
        public string TemplateName => Evaluate("Context.TemplateName") as string;

        /// <summary>
        /// The name of the zone which is rendered.
        /// </summary>
        public string ZoneName { get; private set; }

        /// <summary>
        /// The template model
        /// </summary>
        public IDictionary<string, object> Model { get; private set; }

        /// <summary>
        /// Evaluates an expression - e.g. Product.Sku - and returns it's value.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public object Evaluate(string expression)
        {
            return LiquidContext[expression, false];
        }

        /// <summary>
        /// Specifies the custom content which the template engine should parse and inject.
        /// </summary>
        /// <param name="content">The content</param>
        public void InjectContent(string content)
        {
            if (content.HasValue())
            {
                AddSnippet(new Snippet { Content = content, Parse = true });
            }
        }

        /// <summary>
        /// Specifies the custom content to inject.
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="parse">This should be <c>true</c> if the content contains template syntax.</param>
        public void InjectContent(string content, bool parse)
        {
            if (content.HasValue())
            {
                AddSnippet(new Snippet { Content = content, Parse = parse });
            }
        }

        private void AddSnippet(Snippet snippet)
        {
            if (_snippets == null)
                _snippets = new List<Snippet>();

            _snippets.Add(snippet);
        }

        internal IList<Snippet> Snippets => _snippets;

        internal class Snippet
        {
            public string Content { get; set; }
            public bool Parse { get; set; }
        }
    }
}
