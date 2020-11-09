using System;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Security
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SanitizeHtmlAttribute : Attribute, IMetadataAware
    {
        public SanitizeHtmlAttribute()
        {
            IsFragment = true;
        }

        public bool IsFragment
        {
            get;
            set;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            Guard.NotNull(metadata, nameof(metadata));

            metadata.RequestValidationEnabled = false;
        }
    }
}
