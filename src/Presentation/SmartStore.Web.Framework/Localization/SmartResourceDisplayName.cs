using System.Runtime.CompilerServices;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework
{
    public class SmartResourceDisplayName : System.ComponentModel.DisplayNameAttribute, IModelAttribute
    {
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
                string value = null;
                var langId = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;
                value = EngineContext.Current.Resolve<ILocalizationService>().GetResource(ResourceKey, langId, true, "" /* ResourceKey */, true);

                if (value.IsEmpty() && _callerPropertyName.HasValue())
                {
                    value = _callerPropertyName.SplitPascalCase();
                }

                return value;
            }
        }

        public string Name => "SmartResourceDisplayName";
    }
}
