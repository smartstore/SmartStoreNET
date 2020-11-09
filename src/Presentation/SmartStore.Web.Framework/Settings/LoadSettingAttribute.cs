using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using SmartStore.Core.Configuration;
using SmartStore.Services;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Framework.Settings
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class LoadSettingAttribute : FilterAttribute, IActionFilter
    {
        public sealed class SettingParam
        {
            public ISettings Instance { get; set; }
            public ParameterDescriptor Parameter { get; set; }
        }

        protected int _storeId;
        protected SettingParam[] _settingParams;

        public LoadSettingAttribute()
            : this(true)
        {
        }

        public LoadSettingAttribute(bool updateParameterFromStore)
        {
            UpdateParameterFromStore = updateParameterFromStore;
        }

        public bool UpdateParameterFromStore { get; set; }
        public bool IsRootedModel { get; set; }
        public ICommonServices Services { get; set; }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Get the current configured store id
            _storeId = filterContext.Controller.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            Func<ParameterDescriptor, bool> predicate = (x) => new[] { "storescope", "storeid" }.Contains(x.ParameterName, StringComparer.OrdinalIgnoreCase);
            var storeScopeParam = FindActionParameters<int>(filterContext.ActionDescriptor, false, false, predicate).FirstOrDefault();
            if (storeScopeParam != null)
            {
                // We found an action param named storeScope with type int. Assign our storeId to it.
                filterContext.ActionParameters[storeScopeParam.ParameterName] = _storeId;
            }

            // Find the required ISettings concrete types in ActionDescriptor.GetParameters()
            _settingParams = FindActionParameters<ISettings>(filterContext.ActionDescriptor)
                .Select(x =>
                {
                    // Load settings for the settings type obtained with FindActionParameters<ISettings>()
                    var settings = UpdateParameterFromStore
                        ? Services.Settings.LoadSetting(x.ParameterType, _storeId)
                        : filterContext.ActionParameters[x.ParameterName] as ISettings;

                    if (settings == null)
                    {
                        throw new InvalidOperationException($"Could not load settings for type '{x.ParameterType.FullName}'.");
                    }

                    // Replace settings from action parameters with our loaded settings
                    if (UpdateParameterFromStore)
                    {
                        filterContext.ActionParameters[x.ParameterName] = settings;
                    }

                    return new SettingParam
                    {
                        Instance = settings,
                        Parameter = x
                    };
                })
                .ToArray();
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResultBase;

            if (viewResult != null)
            {
                var model = viewResult.Model;

                if (model == null)
                {
                    // Nothing to override. E.g. insufficient permission.
                    return;
                }

                var modelType = model.GetType();
                var settingsHelper = new StoreDependingSettingHelper(filterContext.Controller.ViewData);
                if (IsRootedModel)
                {
                    settingsHelper.CreateViewDataObject(_storeId);
                }

                foreach (var param in _settingParams)
                {
                    var settingInstance = param.Instance;
                    var modelInstance = model;
                    
                    if (IsRootedModel)
                    {
                        modelInstance = modelType.GetProperty(settingInstance.GetType().Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(model);
                        if (modelInstance == null)
                        {
                            continue;
                        }
                    }

                    settingsHelper.GetOverrideKeys(settingInstance, modelInstance, _storeId, Services.Settings, !IsRootedModel);
                }
            }
        }

        protected IEnumerable<ParameterDescriptor> FindActionParameters<T>(
            ActionDescriptor actionDescriptor,
            bool requireDefaultConstructor = true,
            bool throwIfNotFound = true,
            Func<ParameterDescriptor, bool> predicate = null)
        {
            Guard.NotNull(actionDescriptor, nameof(actionDescriptor));

            var t = typeof(T);

            var query = actionDescriptor
                .GetParameters()
                .Where(x => t.IsAssignableFrom(x.ParameterType));

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (throwIfNotFound && !query.Any())
            {
                throw new InvalidOperationException(
                    $"A controller action method with a '{this.GetType().Name}' attribute requires an action parameter of type '{t.Name}' in order to execute properly.");
            }

            if (requireDefaultConstructor)
            {
                foreach (var param in query)
                {
                    if (!param.ParameterType.HasDefaultConstructor())
                    {
                        throw new InvalidOperationException($"The parameter '{param.ParameterName}' must have a default parameterless constructor.");
                    }
                }
            }

            return query;
        }
    }
}
