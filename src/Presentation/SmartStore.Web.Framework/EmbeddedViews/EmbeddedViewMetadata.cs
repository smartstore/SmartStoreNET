using System;

namespace SmartStore.Web.Framework.EmbeddedViews
{
    [Serializable]
    public class EmbeddedViewMetadata
    {
        public string Name { get; set; }
        public string AssemblyFullName { get; set; }
    }
}