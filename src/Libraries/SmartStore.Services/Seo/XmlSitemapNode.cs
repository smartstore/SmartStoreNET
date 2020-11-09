using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Seo
{
    public class XmlSitemapNode
    {
        public string Loc { get; set; }
        public DateTime? LastMod { get; set; }
        public ChangeFrequency? ChangeFreq { get; set; }
        public float? Priority { get; set; }
        public IEnumerable<LinkEntry> Links { get; set; }

        public class LinkEntry
        {
            public string Lang { get; set; }
            public string Href { get; set; }
        }
    }

    /// <summary>
    /// Represents a sitemap update frequency
    /// </summary>
    public enum ChangeFrequency
    {
        /// <summary>
        /// Always
        /// </summary>
        Always,
        /// <summary>
        /// Hourly
        /// </summary>
        Hourly,
        /// <summary>
        /// Daily
        /// </summary>
        Daily,
        /// <summary>
        /// Weekly
        /// </summary>
        Weekly,
        /// <summary>
        /// Monthly
        /// </summary>
        Monthly,
        /// <summary>
        /// Yearly
        /// </summary>
        Yearly,
        /// <summary>
        /// Never
        /// </summary>
        Never
    }
}
