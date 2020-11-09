using System;
using System.Globalization;
using System.Web.ModelBinding;

namespace SmartStore.Web.Framework.Localization
{
    public class ErrorMessageProvider
    {
        private static string _resourceClassKey;

        public static void SetResourceClassKey(string resourceClassKey)
        {
            if (resourceClassKey == null)
            {
                throw new ArgumentNullException("resourceClassKey");
            }

            _resourceClassKey = resourceClassKey;

            ModelBinderErrorMessageProviders.ValueRequiredErrorMessageProvider = ErrorMessageProvider.DefaultValueRequiredErrorMessageProvider;
            ModelBinderErrorMessageProviders.TypeConversionErrorMessageProvider = ErrorMessageProvider.DefaultTypeConversionErrorMessageProvider;
        }

        private static string DefaultTypeConversionErrorMessageProvider(ModelBindingExecutionContext modelBindingExecutionContext, ModelMetadata modelMetadata, object incomingValue)
        {
            return GetResourceCommon(modelBindingExecutionContext, modelMetadata, incomingValue, new Func<ModelBindingExecutionContext, string>(GetValueInvalidResource));
        }

        private static string DefaultValueRequiredErrorMessageProvider(ModelBindingExecutionContext modelBindingExecutionContext, ModelMetadata modelMetadata, object incomingValue)
        {
            return GetResourceCommon(modelBindingExecutionContext, modelMetadata, incomingValue, new Func<ModelBindingExecutionContext, string>(GetValueRequiredResource));
        }

        private static string GetResourceCommon(ModelBindingExecutionContext modelBindingExecutionContext, ModelMetadata modelMetadata, object incomingValue,
            Func<ModelBindingExecutionContext, string> resourceAccessor)
        {
            string displayName = modelMetadata.GetDisplayName();
            string str = resourceAccessor(modelBindingExecutionContext);
            object[] objArray = new object[2];

            objArray[0] = incomingValue;
            objArray[1] = displayName;

            string str1 = string.Format(CultureInfo.CurrentCulture, str, objArray);
            return str1;
        }

        private static string GetUserResourceString(ModelBindingExecutionContext modelBindingExecutionContext, string resourceName, string resourceClassKey)
        {
            if (string.IsNullOrEmpty(resourceClassKey) || modelBindingExecutionContext == null || modelBindingExecutionContext.HttpContext == null)
            {
                return null;
            }
            else
            {
                return modelBindingExecutionContext.HttpContext.GetGlobalResourceObject(resourceClassKey, resourceName, CultureInfo.CurrentUICulture) as string;
            }
        }

        private static string GetUserResourceString(ModelBindingExecutionContext modelBindingExecutionContext, string resourceName)
        {
            return GetUserResourceString(modelBindingExecutionContext, resourceName, _resourceClassKey);
        }

        private static string GetValueInvalidResource(ModelBindingExecutionContext modelBindingExecutionContext)
        {
            string userResourceString = GetUserResourceString(modelBindingExecutionContext, "PropertyValueInvalid");
            return userResourceString;
        }

        private static string GetValueRequiredResource(ModelBindingExecutionContext modelBindingExecutionContext)
        {
            string userResourceString = GetUserResourceString(modelBindingExecutionContext, "PropertyValueRequired");
            return userResourceString;
        }
    }
}
