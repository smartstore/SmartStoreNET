using System;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Validators
{
    public class SmartValidatorFactory : AttributedValidatorFactory
    {
        public override IValidator GetValidator(Type type)
        {
            if (type != null)
            {
                var attribute = (ValidatorAttribute)Attribute.GetCustomAttribute(type, typeof(ValidatorAttribute));
                if ((attribute != null) && (attribute.ValidatorType != null))
                {
                    var container = EngineContext.Current.ContainerManager;

                    // Validators can depend on some scoped dependencies settings (such as working language),
                    // that's why we do not cache validators in a singleton cache.
                    var requestCache = container.Resolve<IRequestCache>();

                    string cacheKey = "FluentValidator.{0}".FormatInvariant(attribute.ValidatorType.ToString());
                    var result = requestCache.Get(cacheKey, () =>
                    {
                        var instance = EngineContext.Current.ContainerManager.ResolveUnregistered(attribute.ValidatorType);
                        return instance as IValidator;
                    });

                    return result;
                }
            }

            return null;
        }
    }
}