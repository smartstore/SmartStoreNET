using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web; // codehint: sm-add
using System.Web.Mvc; // codehint: sm-add
using System.Web.Optimization; // codehint: sm-add
using SmartStore.Core;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Web.Framework.UI
{
    public partial class PageTitleBuilder : IPageTitleBuilder
    {
        private readonly HttpContextBase _httpContext; // codehint: sm-add
        private readonly SeoSettings _seoSettings;
        private readonly List<string> _titleParts;
        private readonly List<string> _metaDescriptionParts;
        private readonly List<string> _metaKeywordParts;
        private readonly Dictionary<ResourceLocation, List<string>> _scriptParts;
        private readonly Dictionary<ResourceLocation, List<string>> _cssParts;
        private readonly List<string> _canonicalUrlParts;
        private readonly List<string> _bodyCssClasses; // codehint: sm-add (MC)
		private readonly IStoreContext _storeContext;	// codehint: sm-add

        public PageTitleBuilder(SeoSettings seoSettings, HttpContextBase httpContext,
			IStoreContext storeContext)
        {
            this._httpContext = httpContext; // codehint: sm-add
            this._seoSettings = seoSettings;
            this._titleParts = new List<string>();
            this._metaDescriptionParts = new List<string>();
            this._metaKeywordParts = new List<string>();
            this._scriptParts = new Dictionary<ResourceLocation, List<string>>();
            this._cssParts = new Dictionary<ResourceLocation, List<string>>();
            this._canonicalUrlParts = new List<string>();
            this._bodyCssClasses = new List<string>(); // codehint: sm-add (MC)
			this._storeContext = storeContext;	// codehint: sm-add
        }

        // codehint: sm-add (MC) > helper func; changes all following public funcs to remove code redundancy
        private void AddPartsCore(List<string> list, IEnumerable<string> partsToAdd, bool prepend = false)
        {
            if (partsToAdd != null && partsToAdd.Any())
            {
                if (prepend)
                {
                    // codehing: sm-edit (MC) > insertion of multiple parts at the beginning
                    // should keep order (and not vice-versa as it was originally)
                    list.InsertRange(0, partsToAdd.Where(x => !string.IsNullOrEmpty(x)));
                }
                else
                {
                    list.AddRange(partsToAdd.Where(x => !string.IsNullOrEmpty(x)));
                }

				// themes are store dependent: append store-id so that browser cache holds one less file for each store (and not one for all stores).
				for (int i = 0; i < list.Count; ++i)
				{
					if (list[i].EndsWith(".less", StringComparison.OrdinalIgnoreCase))
						list[i] = "{0}?storeId={1}".FormatWith(list[i], _storeContext.CurrentStore.Id);
				}
            }
        }

        public void AddBodyCssClass(string className)
        {
            if (className.HasValue())
            {
                _bodyCssClasses.Insert(0, className);
            }
        }

        public string GenerateBodyCssClasses()
        {
            if (_bodyCssClasses.Count == 0)
                return null;

            return String.Join(" ", _bodyCssClasses);
        }
        // codehint: sm-add (end)

        public void AddTitleParts(params string[] parts)
        {
            AddPartsCore(_titleParts, parts);
        }
        public void AppendTitleParts(params string[] parts)
        {
            AddPartsCore(_titleParts, parts, true);
        }
        public string GenerateTitle(bool addDefaultTitle)
        {
            string result = "";
            var specificTitle = string.Join(_seoSettings.PageTitleSeparator, _titleParts.AsEnumerable().Reverse().ToArray());
            if (!String.IsNullOrEmpty(specificTitle))
            {
                if (addDefaultTitle)
                {
                    //store name + page title
                    switch (_seoSettings.PageTitleSeoAdjustment)
                    {
                        case PageTitleSeoAdjustment.PagenameAfterStorename:
                            {
                                result = string.Join(_seoSettings.PageTitleSeparator, _seoSettings.DefaultTitle, specificTitle);
                            }
                            break;
                        case PageTitleSeoAdjustment.StorenameAfterPagename:
                        default:
                            {
                                result = string.Join(_seoSettings.PageTitleSeparator, specificTitle, _seoSettings.DefaultTitle);
                            }
                            break;

                    }
                }
                else
                {
                    //page title only
                    result = specificTitle;
                }
            }
            else
            {
                //store name only
                result = _seoSettings.DefaultTitle;
            }
            return result;
        }


        public void AddMetaDescriptionParts(params string[] parts)
        {
            AddPartsCore(_metaDescriptionParts, parts);
        }
        public void AppendMetaDescriptionParts(params string[] parts)
        {
            AddPartsCore(_metaDescriptionParts, parts, true);
        }
        public string GenerateMetaDescription()
        {
            var metaDescription = string.Join(", ", _metaDescriptionParts.AsEnumerable().Reverse().ToArray());
            var result = !String.IsNullOrEmpty(metaDescription) ? metaDescription : _seoSettings.DefaultMetaDescription;
            return result;
        }


        public void AddMetaKeywordParts(params string[] parts)
        {
            AddPartsCore(_metaKeywordParts, parts);
        }
        public void AppendMetaKeywordParts(params string[] parts)
        {
            AddPartsCore(_metaKeywordParts, parts, true);
        }
        public string GenerateMetaKeywords()
        {
            var metaKeyword = string.Join(", ", _metaKeywordParts.AsEnumerable().Reverse().ToArray());
            var result = !String.IsNullOrEmpty(metaKeyword) ? metaKeyword : _seoSettings.DefaultMetaKeywords;
            return result;
        }


        public void AddScriptParts(ResourceLocation location, params string[] parts)
        {
            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<string>());

            AddPartsCore(_scriptParts[location], parts);
        }
        public void AppendScriptParts(ResourceLocation location, params string[] parts)
        {
            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<string>());

            AddPartsCore(_scriptParts[location], parts, true);
        }

        private IEnumerable<string> GetBundleParts(string bundleVirtualPath)
        {
            if (BundleTable.EnableOptimizations)
            {
                yield return BundleTable.Bundles.ResolveBundleUrl(bundleVirtualPath);
                yield break;
            }
            else
            {
                var bundle = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
                if (bundle == null)
                    yield break;
                var bundleParts = BundleResolver.Current.GetBundleContents(bundleVirtualPath);
                foreach (string part in bundleParts)
                {
                    yield return UrlHelper.GenerateContentUrl(part, _httpContext);
                }
            }
        }

        public string GenerateScripts(ResourceLocation location)
        {
            if (!_scriptParts.ContainsKey(location) || _scriptParts[location] == null)
                return "";

            var result = new StringBuilder();
            //use only distinct rows
            foreach (var scriptPath in _scriptParts[location].Distinct())
            {
                // codehint: sm-add (MC)
                string path = scriptPath;
                if (path.StartsWith("~"))
                {
                    foreach (var part in this.GetBundleParts(path))
                    {
                        result.AppendFormat("<script src=\"{0}\" type=\"text/javascript\"></script>", part);
                        result.Append(Environment.NewLine);
                    }
                    continue;
                }
                // codehint: sm-add (end)
                result.AppendFormat("<script src=\"{0}\" type=\"text/javascript\"></script>", path /* codehint: sm-edit */);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }


        public void AddCssFileParts(ResourceLocation location, params string[] parts)
        {
            if (!_cssParts.ContainsKey(location))
                _cssParts.Add(location, new List<string>());

            AddPartsCore(_cssParts[location], parts);
        }
        public void AppendCssFileParts(ResourceLocation location, params string[] parts)
        {
            if (!_cssParts.ContainsKey(location))
                _cssParts.Add(location, new List<string>());

            AddPartsCore(_cssParts[location], parts, true);
        }
        public string GenerateCssFiles(ResourceLocation location)
        {
            if (!_cssParts.ContainsKey(location) || _cssParts[location] == null)
                return "";

            var result = new StringBuilder();
            //use only distinct rows
            foreach (var cssPath in _cssParts[location].Distinct())
            {
                // codehint: sm-add (MC)
                string path = cssPath;
                if (path.StartsWith("~"))
                {
                    foreach (var part in this.GetBundleParts(path))
                    {
                        result.AppendFormat("<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />", part);
                        result.Append(Environment.NewLine);
                    }
                    continue;
                }
                // codehint: sm-add (end)
                result.AppendFormat("<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />", path /* codehint: sm-edit */);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }


        public void AddCanonicalUrlParts(params string[] parts)
        {
            AddPartsCore(_canonicalUrlParts, parts);
        }
        public void AppendCanonicalUrlParts(params string[] parts)
        {
            AddPartsCore(_canonicalUrlParts, parts, true);
        }
        public string GenerateCanonicalUrls()
        {
            var result = new StringBuilder();
            foreach (var canonicalUrl in _canonicalUrlParts)
            {
                result.AppendFormat("<link rel=\"canonical\" href=\"{0}\" />", canonicalUrl);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }

    }
}
