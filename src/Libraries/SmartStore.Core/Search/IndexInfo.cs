using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SmartStore.Utilities;

namespace SmartStore.Core.Search
{
    public enum IndexingStatus
    {
        Unavailable = -1,
        Idle = 0,
        Rebuilding = 1,
        Updating = 2
    }

    public class IndexInfo
    {
        public IndexInfo(string scope)
        {
            Guard.NotEmpty(scope, nameof(scope));

            Scope = scope;
            Fields = new string[0];
        }

        public string Scope { get; private set; }
        public int DocumentCount { get; set; }
        public long IndexSize { get; set; }
        public int LastAddedDocumentId { get; set; }
        public IEnumerable<string> Fields { get; set; }

        // loaded from status file
        public DateTime? LastIndexedUtc { get; set; }
        public TimeSpan? LastIndexingDuration { get; set; }
        public bool IsModified { get; set; }
        public IndexingStatus Status { get; set; }
        public string Error { get; set; }

        /// <summary>
        /// Indicates that the index should be rebuilt from scratch, because
        /// some global settings have changed (like tax rates for example)
        /// </summary>
        public bool ShouldRebuild { get; set; }

        public string ToXml()
        {
            return new XDocument(
                    new XElement("info",
                        new XElement("status", Status),
                        new XElement("last-indexed-utc", LastIndexedUtc?.ToString("u")),
                        new XElement("last-indexing-duration", LastIndexingDuration?.ToString("c")),
                        new XElement("is-modified", IsModified ? "true" : "false"),
                        new XElement("error", Error),
                        new XElement("should-rebuild", ShouldRebuild ? "true" : "false"),
                        new XElement("document-count", DocumentCount),
                        new XElement("index-size", IndexSize),
                        new XElement("last-added-document-id", LastAddedDocumentId),
                        new XElement("fields", string.Join(", ", Fields ?? Enumerable.Empty<string>()))
            )).ToString();
        }

        public static IndexInfo FromXml(string xml, string scope)
        {
            var info = new IndexInfo(scope);

            try
            {
                var doc = XDocument.Parse(xml);

                var lastIndexed = doc.Descendants("last-indexed-utc").FirstOrDefault()?.Value;
                if (lastIndexed.HasValue())
                {
                    info.LastIndexedUtc = lastIndexed.Convert<DateTime?>()?.ToUniversalTime();
                }

                var lastDuration = doc.Descendants("last-indexing-duration").FirstOrDefault()?.Value;
                if (lastDuration.HasValue())
                {
                    info.LastIndexingDuration = lastDuration.Convert<TimeSpan?>();
                }

                var isModified = doc.Descendants("is-modified").FirstOrDefault()?.Value;
                if (isModified.HasValue())
                {
                    info.IsModified = isModified.Convert<bool>();
                }
                else
                {
                    info.IsModified = lastIndexed.HasValue();
                }

                var status = doc.Descendants("status").FirstOrDefault()?.Value;
                if (status.HasValue())
                {
                    info.Status = status.Convert<IndexingStatus>();
                }

                info.Error = doc.Descendants("error").FirstOrDefault()?.Value;

                var documentCount = doc.Descendants("document-count").FirstOrDefault()?.Value;
                if (documentCount.HasValue())
                {
                    info.DocumentCount = documentCount.ToInt();
                }

                var indexSize = doc.Descendants("index-size").FirstOrDefault()?.Value;
                if (indexSize.HasValue() && CommonHelper.TryConvert(indexSize, out long size))
                {
                    info.IndexSize = size;
                }

                var lastAddedDocumentId = doc.Descendants("last-added-document-id").FirstOrDefault()?.Value;
                if (lastAddedDocumentId.HasValue())
                {
                    info.LastAddedDocumentId = lastAddedDocumentId.ToInt();
                }

                var fields = doc.Descendants("fields").FirstOrDefault()?.Value;
                if (fields.HasValue())
                {
                    info.Fields = fields.SplitSafe(", ");
                }

                var shouldRebuild = doc.Descendants("should-rebuild").FirstOrDefault()?.Value;
                if (shouldRebuild.HasValue())
                {
                    info.ShouldRebuild = shouldRebuild.Convert<bool>();
                }
            }
            catch { }

            return info;
        }
    }
}
