using System;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.UI
{
    public static class LayoutExtensions
    {
        #region TitleParts

        public static void AddTitleParts(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder  = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AddTitleParts(parts);
        }

        public static void AppendTitleParts(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder  = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AppendTitleParts(parts);
        }

        public static MvcHtmlString SmartTitle(this HtmlHelper html, bool addDefaultTitle, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            html.AppendTitleParts(parts);
            return MvcHtmlString.Create(html.Encode(pageAssetsBuilder.GenerateTitle(addDefaultTitle)));
        }

        #endregion

        #region MetaDescriptionParts

        public static void AddMetaDescriptionParts(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AddMetaDescriptionParts(parts);
        }

        public static void AppendMetaDescriptionParts(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AppendMetaDescriptionParts(parts);
        }

        public static MvcHtmlString SmartMetaDescription(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            html.AppendMetaDescriptionParts(parts);
            return MvcHtmlString.Create(html.Encode(pageAssetsBuilder.GenerateMetaDescription()));
        }

        #endregion

        #region MetaKeywordParts

        public static void AddMetaKeywordParts(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AddMetaKeywordParts(parts);
        }

        public static void AppendMetaKeywordParts(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AppendMetaKeywordParts(parts);
        }

        public static MvcHtmlString SmartMetaKeywords(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            html.AppendMetaKeywordParts(parts);
            return MvcHtmlString.Create(html.Encode(pageAssetsBuilder.GenerateMetaKeywords()));
        }

		#endregion

		#region MetaMisc

		public static MvcHtmlString SmartMetaRobots(this HtmlHelper html)
		{
			var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			return MvcHtmlString.Create(pageAssetsBuilder.GenerateMetaRobots());
		}

		#endregion

		#region ScriptParts

		public static void AddScriptParts(this HtmlHelper html, params string[] parts)
        {
            AddScriptParts(html, ResourceLocation.Foot, false, parts);
        }

        public static void AddScriptParts(this HtmlHelper html, ResourceLocation location, params string[] parts)
        {
            AddScriptParts(html, location, false, parts);
        }

        public static void AddScriptParts(this HtmlHelper html, bool excludeFromBundling, params string[] parts)
        {
            AddScriptParts(html, ResourceLocation.Foot, excludeFromBundling, parts);
        }

        public static void AddScriptParts(this HtmlHelper html, ResourceLocation location, bool excludeFromBundling, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AddScriptParts(location, excludeFromBundling, parts);
        }

        public static void AppendScriptParts(this HtmlHelper html, params string[] parts)
        {
            AppendScriptParts(html, ResourceLocation.Foot, false, parts);
        }

        public static void AppendScriptParts(this HtmlHelper html, ResourceLocation location, params string[] parts)
        {
            AppendScriptParts(html, location, false, parts);
        }

        public static void AppendScriptParts(this HtmlHelper html, bool excludeFromBundling, params string[] parts)
        {
            AppendScriptParts(html, ResourceLocation.Foot, excludeFromBundling, parts);
        }

        public static void AppendScriptParts(this HtmlHelper html, ResourceLocation location, bool excludeFromBundling, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AppendScriptParts(location, excludeFromBundling, parts);
        }

        public static MvcHtmlString SmartScripts(this HtmlHelper html, UrlHelper urlHelper, ResourceLocation location, bool? enableBundling = null)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            return MvcHtmlString.Create(pageAssetsBuilder.GenerateScripts(urlHelper, location, enableBundling));
        }

        #endregion

        #region CssFileParts

        public static void AddCssFileParts(this HtmlHelper html, params string[] parts)
        {
            AddCssFileParts(html, ResourceLocation.Head, false, parts);
        }

        public static void AddCssFileParts(this HtmlHelper html, ResourceLocation location, params string[] parts)
        {
            AddCssFileParts(html, location, false, parts);
        }

        public static void AddCssFileParts(this HtmlHelper html, bool excludeFromBundling, params string[] parts)
        {
            AddCssFileParts(html, ResourceLocation.Head, excludeFromBundling, parts);
        }

        public static void AddCssFileParts(this HtmlHelper html, ResourceLocation location, bool excludeFromBundling, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AddCssFileParts(location, excludeFromBundling, parts);
        }

        public static void AppendCssFileParts(this HtmlHelper html, params string[] parts)
        {
            AppendCssFileParts(html, ResourceLocation.Head, false, parts);
        }

        public static void AppendCssFileParts(this HtmlHelper html, ResourceLocation location, params string[] parts)
        {
            AppendCssFileParts(html, location, false, parts);
        }

        public static void AppendCssFileParts(this HtmlHelper html, bool excludeFromBundling, params string[] parts)
        {
            AppendCssFileParts(html, ResourceLocation.Head, excludeFromBundling, parts);
        }

        public static void AppendCssFileParts(this HtmlHelper html, ResourceLocation location, bool excludeFromBundling, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AppendCssFileParts(location, excludeFromBundling, parts);
        }

        public static MvcHtmlString SmartCssFiles(this HtmlHelper html, UrlHelper urlHelper, ResourceLocation location, bool? enableBundling = null)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            return MvcHtmlString.Create(pageAssetsBuilder.GenerateCssFiles(urlHelper, location, enableBundling));
        }

        #endregion

        #region CustomHeadParts

        public static void AddCustomHeadParts(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AddCustomHeadParts(parts);
        }

        public static void AppendCustomHeadParts(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			pageAssetsBuilder.AppendCustomHeadParts(parts);
        }

        public static MvcHtmlString CustomHead(this HtmlHelper html, params string[] parts)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			html.AppendCustomHeadParts(parts);
            return MvcHtmlString.Create(pageAssetsBuilder.GenerateCustomHead());
		}

        #endregion

		#region CanonicalUrlParts

		public static void AddCanonicalUrlParts(this HtmlHelper html, params string[] parts)
		{
			var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			pageAssetsBuilder.AddCanonicalUrlParts(parts);
		}

		public static void AppendCanonicalUrlParts(this HtmlHelper html, params string[] parts)
		{
			var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			pageAssetsBuilder.AppendCanonicalUrlParts(parts);
		}

		public static MvcHtmlString CanonicalUrls(this HtmlHelper html, params string[] parts)
		{
			var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			html.AppendCanonicalUrlParts(parts);
			return MvcHtmlString.Create(pageAssetsBuilder.GenerateCanonicalUrls());
		}

		#endregion

		#region LinkParts

		public static void AddLinkPart(this HtmlHelper html, string rel, string href, string type = null, string media = null, string sizes = null, string hreflang = null)
		{
			var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			pageAssetsBuilder.AddLinkPart(rel, href, type, media, sizes, hreflang);
		}

		public static void AddLinkPart(this HtmlHelper html, string rel, string href, object htmlAttributes)
		{
			var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			pageAssetsBuilder.AddLinkPart(rel, href, htmlAttributes);
		}

		public static void AddLinkPart(this HtmlHelper html, string rel, string href, RouteValueDictionary htmlAttributes)
		{
			var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			pageAssetsBuilder.AddLinkPart(rel, href, htmlAttributes);
		}

		public static MvcHtmlString LinkRels(this HtmlHelper html)
		{
			var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
			return MvcHtmlString.Create(pageAssetsBuilder.GenerateLinkRels());
		}

		#endregion

		#region Body

		public static void AddBodyCssClass(this HtmlHelper html, string cssClassName)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            pageAssetsBuilder.AddBodyCssClass(cssClassName);
        }

        public static MvcHtmlString BodyCssClass(this HtmlHelper html)
        {
            var pageAssetsBuilder = EngineContext.Current.Resolve<IPageAssetsBuilder>();
            return MvcHtmlString.Create(html.Encode(pageAssetsBuilder.GenerateBodyCssClasses()));
        }

		public static MvcHtmlString BodyId(this HtmlHelper html)
		{
			string result = "";

			try
			{
				var storeContext = EngineContext.Current.Resolve<IStoreContext>();
				result = storeContext.CurrentStore.HtmlBodyId.ToAttribute("id");
			}
			catch { }

			return MvcHtmlString.Create(result);
        }

        #endregion
    }
}
