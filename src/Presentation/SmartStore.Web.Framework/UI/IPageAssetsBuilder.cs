using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
    
    public partial interface IPageAssetsBuilder
    {
        void AddTitleParts(IEnumerable<string> parts, bool append = false);
        void AddMetaDescriptionParts(IEnumerable<string> parts, bool append = false);
        void AddMetaKeywordParts(IEnumerable<string> parts, bool append = false);
        void AddCanonicalUrlParts(IEnumerable<string> parts, bool append = false);
        void AddBodyCssClass(string className);
        void AddScriptParts(ResourceLocation location, IEnumerable<string> parts, bool excludeFromBundling = false, bool append = false);
        void AddCssFileParts(ResourceLocation location, IEnumerable<string> parts, bool excludeFromBundling = false, bool append = false);

        string GenerateTitle(bool addDefaultTitle);
        string GenerateMetaDescription();
        string GenerateMetaKeywords();
        string GenerateCanonicalUrls();
        string GenerateBodyCssClasses();
        string GenerateScripts(UrlHelper urlHelper, ResourceLocation location, bool? enableBundling = null);
        string GenerateCssFiles(UrlHelper urlHelper, ResourceLocation location, bool? enableBundling = null);
    }

    public static class PageAssetsBuilderExtensions
    {
        public static void AddTitleParts(this IPageAssetsBuilder builder, params string[] parts)
        {
            builder.AddTitleParts(parts);
        }

        public static void AppendTitleParts(this IPageAssetsBuilder builder, params string[] parts)
        {
            builder.AddTitleParts(parts, true);
        }

        public static void AddMetaDescriptionParts(this IPageAssetsBuilder builder, params string[] parts)
        {
            builder.AddMetaDescriptionParts(parts);
        }

        public static void AppendMetaDescriptionParts(this IPageAssetsBuilder builder, params string[] parts)
        {
            builder.AddMetaDescriptionParts(parts, true);
        }

        public static void AddMetaKeywordParts(this IPageAssetsBuilder builder, params string[] parts)
        {
            builder.AddMetaKeywordParts(parts);
        }

        public static void AppendMetaKeywordParts(this IPageAssetsBuilder builder, params string[] parts)
        {
            builder.AddMetaKeywordParts(parts, true);
        }

        public static void AddCanonicalUrlParts(this IPageAssetsBuilder builder, params string[] parts)
        {
            builder.AddCanonicalUrlParts(parts);
        }

        public static void AppendCanonicalUrlParts(this IPageAssetsBuilder builder, params string[] parts)
        {
            builder.AddCanonicalUrlParts(parts, true);
        }
        

        public static void AddScriptParts(this IPageAssetsBuilder builder, ResourceLocation location, params string[] parts)
        {
            builder.AddScriptParts(location, parts, false, false);
        }

        public static void AddScriptParts(this IPageAssetsBuilder builder, ResourceLocation location, bool excludeFromBundling, params string[] parts)
        {
            builder.AddScriptParts(location, parts, excludeFromBundling, false);
        }

        public static void AppendScriptParts(this IPageAssetsBuilder builder, ResourceLocation location, params string[] parts)
        {
            builder.AddScriptParts(location, parts, false, true);
        }

        public static void AppendScriptParts(this IPageAssetsBuilder builder, ResourceLocation location, bool excludeFromBundling, params string[] parts)
        {
            builder.AddScriptParts(location, parts, excludeFromBundling, true);
        }


        public static void AddCssFileParts(this IPageAssetsBuilder builder, ResourceLocation location, params string[] parts)
        {
            builder.AddCssFileParts(location, parts, false, false);
        }

        public static void AddCssFileParts(this IPageAssetsBuilder builder, ResourceLocation location, bool excludeFromBundling, params string[] parts)
        {
            builder.AddCssFileParts(location, parts, excludeFromBundling, false);
        }

        public static void AppendCssFileParts(this IPageAssetsBuilder builder, ResourceLocation location, params string[] parts)
        {
            builder.AddCssFileParts(location, parts, false, true);
        }

        public static void AppendCssFileParts(this IPageAssetsBuilder builder, ResourceLocation location, bool excludeFromBundling, params string[] parts)
        {
            builder.AddCssFileParts(location, parts, excludeFromBundling, true);
        }
    }
}
