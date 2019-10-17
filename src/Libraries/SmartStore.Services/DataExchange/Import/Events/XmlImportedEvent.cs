using System.Collections.Generic;
using System.Xml.XPath;

namespace SmartStore.Services.DataExchange.Import.Events
{
	public class XmlImportedEvent
    {
        /// <summary>
        /// Used to provide additionally exported xml data to event consumers (e.g. Plugins)
        /// </summary>
        /// <param name="data">Additional data</param>
        /// <param name="tagname">Name of the xml node</param>
        public XmlImportedEvent(Dictionary<int, XPathNavigator> data, string tagname)
        {
            Guard.NotEmpty(data, nameof(data));
            Guard.NotNull(tagname, nameof(tagname));

            Data = data;
            TagName = tagname;
        }

        /// <summary>
        /// Additional data
        /// </summary>
        public Dictionary<int, XPathNavigator> Data
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the xml node (e.g. PluginName)
        /// </summary>
        public string TagName
        {
            get;
            set;
        }
    }
}
