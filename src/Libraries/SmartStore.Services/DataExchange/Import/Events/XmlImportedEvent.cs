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
        public XmlImportedEvent(Dictionary<int, AdditionalXmlData> data, string tagname)
        {
            Guard.NotEmpty(data, nameof(data));
            Guard.NotNull(tagname, nameof(tagname));

            Data = data;
            TagName = tagname;
        }

        /// <summary>
        /// Additional data
        /// </summary>
        public Dictionary<int, AdditionalXmlData> Data
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


    public class AdditionalXmlData
    {
        public AdditionalXmlData(int exportedEntityId, int importedEntityId, XPathNavigator data)
        {
            Guard.NotNull(data, nameof(data));

            ExportedEntityId = exportedEntityId;
            ImportedEntityId = importedEntityId;
            Data = data;
        }

        /// <summary>
        /// The original, exported ID of the entity.
        /// </summary>
        public int ExportedEntityId
        {
            get;
            private set;
        }

        /// <summary>
        /// The ID of the imported entity.
        /// </summary>
        public int ImportedEntityId
        {
            get;
            private set;
        }

        /// <summary>
        /// Additional XML data.
        /// </summary>
        public XPathNavigator Data
        {
            get;
            private set;
        }
    }
}
