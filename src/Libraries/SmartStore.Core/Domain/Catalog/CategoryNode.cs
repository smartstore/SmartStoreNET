using System;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Catalog
{
    public interface ICategoryNode : ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported
    {
        int ParentCategoryId { get; }
        string Name { get; }
        string ExternalLink { get; }
        string Alias { get; }
        int? MediaFileId { get; }
        bool Published { get; }
        int DisplayOrder { get; }
        DateTime UpdatedOnUtc { get; }
        string BadgeText { get; }
        int BadgeStyle { get; }
    }

    [Serializable]
    public class CategoryNode : ICategoryNode
    {
        public int Id { get; set; }
        public int ParentCategoryId { get; set; }
        public string Name { get; set; }
        public string ExternalLink { get; set; }
        public string Alias { get; set; }
        public int? MediaFileId { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string BadgeText { get; set; }
        public int BadgeStyle { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
    }
}
