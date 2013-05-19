using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Localization
{
    
    public class LocalizedEntityPostDeleteHook : PostDeleteHook<ILocalizedEntity>
    {
        private readonly ILocalizedEntityService _localizedEntityService;

        public LocalizedEntityPostDeleteHook(ILocalizedEntityService localizedEntityService)
        {
            this._localizedEntityService = localizedEntityService;
        }

        public override void Hook(ILocalizedEntity entity, HookEntityMetadata metadata)
        {
            var baseEntity = entity as BaseEntity;

            if (baseEntity == null)
                return;

            var entityType = baseEntity.GetType();
            if (entityType.AssemblyQualifiedName.StartsWith("System.Data.Entity."))
            {
                // it's a proxied type
                entityType = entityType.BaseType;
            }
            var localizedEntities = this._localizedEntityService.GetLocalizedProperties(baseEntity.Id, entityType.Name);

            localizedEntities.Each(x => this._localizedEntityService.DeleteLocalizedProperty(x));
        }
    }

}
