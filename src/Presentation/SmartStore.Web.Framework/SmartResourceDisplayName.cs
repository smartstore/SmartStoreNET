using System;
using System.Runtime.CompilerServices;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Framework
{
    public class SmartResourceDisplayName : System.ComponentModel.DisplayNameAttribute, IModelAttribute
    {
        //private string _resourceValue = string.Empty;
        //private bool _resourceValueRetrived;

        private readonly string _callerPropertyName;

        public SmartResourceDisplayName(string resourceKey, [CallerMemberName] string propertyName = null)
            : base(resourceKey)
        {
            ResourceKey = resourceKey;
            _callerPropertyName = propertyName;
        }

        public string ResourceKey { get; set; }

        public override string DisplayName
        {
            get
            {
                //do not cache resources because it causes issues when you have multiple languages
                //if (!_resourceValueRetrived)
                //{
                string value = null;
                var langId = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;
                value = EngineContext.Current.Resolve<ILocalizationService>()
                        .GetResource(ResourceKey, langId, true, "" /* ResourceKey */, true);

                if (value.IsEmpty() && _callerPropertyName.HasValue())
                {
                    value = _callerPropertyName.SplitPascalCase();
                }

                return value;

                //    _resourceValueRetrived = true;
                //}
                //return _resourceValue;
            }
        }

        public string Name
        {
            get { return "SmartResourceDisplayName"; }
        }
    }
}
