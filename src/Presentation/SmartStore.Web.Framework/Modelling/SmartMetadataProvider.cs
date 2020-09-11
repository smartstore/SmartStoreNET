using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Modelling
{
    /// <summary>
	/// This MetadataProvider adds some functionality on top of the default CachedDataAnnotationsModelMetadataProvider.
    /// It adds custom attributes (implementing IModelAttribute) to the AdditionalValues property of the model's metadata
    /// so that it can be retrieved later.
    /// </summary>
    public class SmartMetadataProvider : CachedDataAnnotationsModelMetadataProvider
    {
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        protected override CachedDataAnnotationsModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName)
        {
            var metadata = base.CreateMetadataPrototype(attributes, containerType, modelType, propertyName);
            var attrs = attributes.OfType<IModelAttribute>().ToList();

            foreach (var attr in attrs)
            {
                if (metadata.AdditionalValues.ContainsKey(attr.Name))
                {
                    throw new SmartException("There is already an attribute with the name of '{0}' on this model.".FormatCurrent(attr.Name));
                }
                metadata.AdditionalValues.Add(attr.Name, attr);
            }

            return metadata;
        }

        protected override CachedDataAnnotationsModelMetadata CreateMetadataFromPrototype(CachedDataAnnotationsModelMetadata prototype, Func<object> modelAccessor)
        {
            var result = base.CreateMetadataFromPrototype(prototype, modelAccessor);

            if (prototype.AdditionalValues.Count > 0)
            {
                var attrs = prototype.AdditionalValues.Where(x => x.Value is IModelAttribute).ToArray();
                if (attrs.Any())
                {
                    result.AdditionalValues.AddRange(attrs);
                }
            }

            return result;
        }
    }
}