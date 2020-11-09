using System;
using SmartStore.Core;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Services.Seo
{
    public class NamedEntity : BaseEntity, ISlugSupported
    {
        public string EntityName { get; set; }
        public string DisplayName { get; set; }
        public string Slug { get; set; }
        public DateTime LastMod { get; set; }
        public int? LanguageId { get; set; }

        public override string GetEntityName()
        {
            return EntityName;
        }
    }
}
