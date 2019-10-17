using SmartStore.Collections;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SmartStore.Services.DataExchange.Export.Events
{
	public class XmlExportingEvent
    {
        /// <summary>
        /// Used to inject data into an XML export process
        /// </summary>
        /// <param name="entityIds">Ids of the currently exported entities</param>
        public XmlExportingEvent(IEnumerable<int> entityIds)
        {
            Guard.NotNull(entityIds, nameof(entityIds));

            EntityIds = entityIds;

            Injectors = new List<XmlExportingEventInjector>();
        }

        /// <summary>
        /// List of XML injectors (event consumers)
        /// </summary>
        public List<XmlExportingEventInjector> Injectors { get; private set; }

        /// <summary>
        /// Ids of the currently exported entities
        /// </summary>
        public IEnumerable<int> EntityIds
        {
            get;
            private set;
        }
    }

    public class XmlExportingEventInjector
    {
        /// <summary>
        /// Represents an injector (consumer of the event)
        /// </summary>
        /// <param name="tagname">Name of the exported tag e.g. PluginName</param>
        public XmlExportingEventInjector(string tagname)
        {
            Guard.NotEmpty(tagname, nameof(tagname));

            XmlWriteActions = new Multimap<int, Action<XmlWriter>>();

            Name = tagname;
        }

        /// <summary>
        /// Adds a delegate to the Multimap of xml writing delegates
        /// </summary>
        /// <param name="productId">Currently exported entity</param>
        /// <param name="action">Event consumer delegate that will populate the currently exported entity node by writing own xml</param>
        public void AddAction(int productId, Action<XmlWriter> action)
        {
            XmlWriteActions.Add(productId, action);
        }

        /// <summary>
        /// Name of the injector e.g. PluginName
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        public int StoreId
        {
            get;
            set;
        }

        /// <summary>
        /// Multimap of delegates that will populate the currently exported entity node by writing own xml
        /// </summary>
        public Multimap<int, Action<XmlWriter>> XmlWriteActions { get; private set; }
    }
}
