using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using SmartStore.Utilities;

// codehint: sm-edit (massively: added proper dictionary binding)

namespace SmartStore.Web.Framework.Mvc
{
    public class SmartModelBinder : DefaultModelBinder
    {
        private readonly IModelBinder _nextBinder;
        private readonly static string s_DictTypeName = typeof(IDictionary<,>).FullName;

        public SmartModelBinder() : this(null)
        {
        }

        public SmartModelBinder(IModelBinder nextBinder)
        {
            _nextBinder = nextBinder;
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            Type modelType = bindingContext.ModelType;

            Type idictType = modelType.GetInterface(s_DictTypeName);

            if (idictType != null)
            {
                var dictInstance = this.BindDictionary(controllerContext, bindingContext, idictType);
                if (dictInstance != null)
                {
                    return dictInstance;
                }
            }

            if (_nextBinder != null)
            {
                return _nextBinder.BindModel(controllerContext, bindingContext);
            }

            var model = base.BindModel(controllerContext, bindingContext);

            if (model is ModelBase)
            {
                ((ModelBase)model).BindModel(controllerContext, bindingContext);
            }

            return model;
        }

        private object BindDictionary(ControllerContext controllerContext, ModelBindingContext bindingContext, Type idictType)
        {
            Type modelType = bindingContext.ModelType;
            object result = null;

            Type[] ga = idictType.GetGenericArguments();
            IModelBinder valueBinder = Binders.GetBinder(ga[1]);

            foreach (string key in GetValueProviderKeys(controllerContext))
            {
                bool isMatch = key.StartsWith(bindingContext.ModelName + "[", StringComparison.InvariantCultureIgnoreCase);
                if (isMatch)
                {
                    int endbracket = key.IndexOf("]", bindingContext.ModelName.Length + 1);
                    if (endbracket == -1)
                        continue;

                    object dictKey;
                    try
                    {
                        dictKey = ConvertType(key.Substring(bindingContext.ModelName.Length + 1, endbracket - bindingContext.ModelName.Length - 1), ga[0]);
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }

                    ModelBindingContext innerBindingContext = new ModelBindingContext()
                    {
                        ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, ga[1]),
                        ModelName = key.Substring(0, endbracket + 1),
                        ModelState = bindingContext.ModelState,
                        PropertyFilter = bindingContext.PropertyFilter,
                        ValueProvider = bindingContext.ValueProvider
                    };
                    object newPropertyValue = valueBinder.BindModel(controllerContext, innerBindingContext);

                    if (result == null)
                        result = CreateModel(controllerContext, bindingContext, modelType);

                    if (!(bool)idictType.GetMethod("ContainsKey").Invoke(result, new object[] { dictKey }))
                        idictType.GetProperty("Item").SetValue(result, ((string[])newPropertyValue)[0], new object[] { dictKey });
                }
            }

            return result;
        }

        private object ConvertType(string stringValue, Type type)
        {
			return CommonHelper.GetTypeConverter(type).ConvertFrom(stringValue);
        }

        private IEnumerable<string> GetValueProviderKeys(ControllerContext context)
        {
            List<string> keys = new List<string>();
            keys.AddRange(context.HttpContext.Request.Form.Keys.Cast<string>());
            keys.AddRange(((IDictionary<string, object>)context.RouteData.Values).Keys.Cast<string>());
            keys.AddRange(context.HttpContext.Request.QueryString.Keys.Cast<string>());
            keys.AddRange(context.HttpContext.Request.Files.Keys.Cast<string>());
            return keys;
        }


    }
}
