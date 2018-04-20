using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Modelling
{
    public class SmartModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
			var modelType = bindingContext.ModelType;

			if (modelType == typeof(CustomPropertiesDictionary))
			{
				return BindCustomPropertiesDictioary(controllerContext, bindingContext);
			}

			var model = base.BindModel(controllerContext, bindingContext);

            if (model is ModelBase)
            {
                ((ModelBase)model).BindModel(controllerContext, bindingContext);
            }

            return model;
        }

		private CustomPropertiesDictionary BindCustomPropertiesDictioary(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var model = bindingContext.Model as CustomPropertiesDictionary ?? new CustomPropertiesDictionary();

			var keys = GetValueProviderKeys(controllerContext, bindingContext.ModelName + "[");
			if (keys.Count == 0)
			{
				return model;
			}

			foreach (var key in keys)
			{
				var keyName = GetKeyName(key);
				if (keyName == null || model.ContainsKey(keyName))
					continue;

				var valueBinder = this.Binders.DefaultBinder;

				var subPropertyName = GetSubPropertyName(key);
				if (subPropertyName.IsCaseInsensitiveEqual("__Type__"))
					continue;

				if (subPropertyName == null)
				{
					var simpleBindingContext = new ModelBindingContext
					{
						ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, GetValueType(keys, key, bindingContext.ValueProvider)),
						ModelName = key,
						ModelState = bindingContext.ModelState,
						PropertyFilter = bindingContext.PropertyFilter,
						ValueProvider = bindingContext.ValueProvider
					};
					var value = valueBinder.BindModel(controllerContext, simpleBindingContext);
					model[keyName] = value;
				}
				else
				{
					// Is Complex type
					var modelName = key.Substring(0, key.Length - subPropertyName.Length - 1);
					var valueType = GetValueType(keys, modelName, bindingContext.ValueProvider);
					valueBinder = this.Binders.GetBinder(valueType);
					var complexBindingContext = new ModelBindingContext
					{
						ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, valueType),
						ModelName = key.Substring(0, key.Length - subPropertyName.Length - 1),
						ModelState = bindingContext.ModelState,
						PropertyFilter = bindingContext.PropertyFilter,
						ValueProvider = bindingContext.ValueProvider
					};
					var value = valueBinder.BindModel(controllerContext, complexBindingContext);
					model[keyName] = value;
				}
			}
			
			return model;
		}

		private HashSet<string> GetValueProviderKeys(ControllerContext context, string prefix)
		{
			var keys = context.HttpContext.Request.Form.Keys.Cast<string>()
				.Concat(((IDictionary<string, object>)context.RouteData.Values).Keys.Cast<string>())
				.Concat(context.HttpContext.Request.QueryString.Keys.Cast<string>())
				.Concat(context.HttpContext.Request.Files.Keys.Cast<string>())
				.Where(x => x.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase));

			return new HashSet<string>(keys, StringComparer.InvariantCultureIgnoreCase);
		}

		private string GetKeyName(string key)
		{
			int startBracket = key.IndexOf("[");
			int endBracket = key.IndexOf("]", startBracket);

			if (endBracket == -1)
				return null;
			
			return key.Substring(startBracket + 1, endBracket - startBracket - 1);
		}

		private string GetSubPropertyName(string key)
		{
			var parts = key.Split('.');
			if (parts.Length > 1)
			{
				return parts[1];
			}

			return null;
		}

		private Type GetValueType(HashSet<string> keys, string prefix, IValueProvider valueProvider)
		{
			var typeKey = prefix + ".__Type__";
			if (keys.Contains(typeKey))
			{
				var type = Type.GetType(valueProvider.GetValue(typeKey).AttemptedValue, true);
				return type;
			}
			return typeof(object);
		}


    }
}
