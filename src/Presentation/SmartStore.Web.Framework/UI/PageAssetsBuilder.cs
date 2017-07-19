using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Web.WebPages;
using System.Web.Mvc;
using System.Web.Optimization;
using SmartStore.Core;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Themes;
using SmartStore.Utilities;
using System.Web.Hosting;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI
{
    public partial class PageAssetsBuilder : IPageAssetsBuilder
    {
        private readonly HttpContextBase _httpContext;
        private readonly SeoSettings _seoSettings;
        private readonly ThemeSettings _themeSettings;
        private readonly List<string> _titleParts;
        private readonly List<string> _metaDescriptionParts;
        private readonly List<string> _metaKeywordParts;
        private readonly List<string> _canonicalUrlParts;
		private readonly List<string> _customHeadParts;
        private readonly List<string> _bodyCssClasses;
        private readonly Dictionary<ResourceLocation, List<WebAssetDescriptor>> _scriptParts;
        private readonly Dictionary<ResourceLocation, List<WebAssetDescriptor>> _cssParts;
		private readonly List<RouteValueDictionary> _linkParts;
		private readonly IStoreContext _storeContext;
        private readonly IBundleBuilder _bundleBuilder;

		private static readonly ConcurrentDictionary<string, string> s_minFiles = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public PageAssetsBuilder(
            SeoSettings seoSettings, 
            ThemeSettings themeSettings,
            HttpContextBase httpContext,
			IStoreContext storeContext,
            IBundleBuilder bundleBuilder)
        {
            _httpContext = httpContext;
            _seoSettings = seoSettings;
            _themeSettings = themeSettings;
            _titleParts = new List<string>();
            _metaDescriptionParts = new List<string>();
            _metaKeywordParts = new List<string>();
            _scriptParts = new Dictionary<ResourceLocation, List<WebAssetDescriptor>>();
            _cssParts = new Dictionary<ResourceLocation, List<WebAssetDescriptor>>();
            _canonicalUrlParts = new List<string>();
			_customHeadParts = new List<string>();
            _bodyCssClasses = new List<string>();
			_linkParts = new List<RouteValueDictionary>();
			_storeContext = storeContext;
            _bundleBuilder = bundleBuilder;
        }

        private bool IsValidPart<T>(T part)
        {
            bool isValid = part != null;
            if (isValid) {
                var str = part as string;
                if (str != null)
                    isValid = str.HasValue();
            }
            return isValid;
        }

        // helper func: changes all following public funcs to remove code redundancy
        private void AddPartsCore<T>(List<T> list, IEnumerable<T> partsToAdd, bool prepend = false)
        {
            if (partsToAdd != null && partsToAdd.Any())
            {
                if (prepend)
                {
                    // insertion of multiple parts at the beginning
                    // should keep order (and not vice-versa as it was originally)
                    list.InsertRange(0, partsToAdd.Where(IsValidPart));
                }
                else
                {
                    list.AddRange(partsToAdd.Where(IsValidPart));
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

        public void AddTitleParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(_titleParts, parts, append);
        }

        public virtual string GenerateTitle(bool addDefaultTitle)
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


        public void AddMetaDescriptionParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(_metaDescriptionParts, parts, append);
        }

        public virtual string GenerateMetaDescription()
        {
            var metaDescription = string.Join(", ", _metaDescriptionParts.AsEnumerable().Reverse().ToArray());
            var result = !String.IsNullOrEmpty(metaDescription) ? metaDescription : _seoSettings.DefaultMetaDescription;
            return result;
        }


        public void AddMetaKeywordParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(_metaKeywordParts, parts, append);
        }

        public virtual string GenerateMetaKeywords()
        {
            var metaKeyword = string.Join(", ", _metaKeywordParts.AsEnumerable().Reverse().ToArray());
            var result = !String.IsNullOrEmpty(metaKeyword) ? metaKeyword : _seoSettings.DefaultMetaKeywords;
            return result;
        }

        public void AddCanonicalUrlParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(_canonicalUrlParts, parts, append);
        }

        public string GenerateCanonicalUrls()
        {
            var result = new StringBuilder();
			var parts = _canonicalUrlParts.Distinct();
			foreach (var part in parts)
            {
                result.AppendFormat("<link rel=\"canonical\" href=\"{0}\" />", part);
                result.AppendLine();
            }
            return result.ToString();
        }

		public void AddCustomHeadParts(IEnumerable<string> parts, bool append = false)
		{
			AddPartsCore(_customHeadParts, parts, append);
		}

		public string GenerateCustomHead()
		{
			var result = new StringBuilder();
			var parts = _customHeadParts.Distinct();
			foreach (var part in parts)
			{
				result.AppendLine(part);
			}
			return result.ToString();
		}

        public void AddScriptParts(ResourceLocation location, IEnumerable<string> parts, bool excludeFromBundling = false, bool append = false)
        {
            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<WebAssetDescriptor>());

            var descriptors = parts.Select(x => new WebAssetDescriptor { 
                ExcludeFromBundling = excludeFromBundling || !x.StartsWith("~") || BundleTable.Bundles.GetBundleFor(x) != null, 
                Part = x });

            AddPartsCore(_scriptParts[location], descriptors, append);
        }

        public string GenerateScripts(UrlHelper urlHelper, ResourceLocation location, bool? enableBundling = null)
        {
            if (!_scriptParts.ContainsKey(location) || _scriptParts[location] == null)
                return "";

            if (_scriptParts[location].Count == 0)
                return "";

            if (!enableBundling.HasValue)
            {
                enableBundling = this.BundlingEnabled;
            }

            var prevEnableOptimizations = BundleTable.EnableOptimizations;
            BundleTable.EnableOptimizations = enableBundling.Value;

            var parts = _scriptParts[location];
            var bundledParts = parts.Where(x => !x.ExcludeFromBundling).Select(x => x.Part).Distinct();
            var nonBundledParts = parts.Where(x => x.ExcludeFromBundling).Select(x => x.Part).Distinct();
            
            var sb = new StringBuilder();

            if (bundledParts.Any())
            {
                sb.AppendLine(_bundleBuilder.Build(BundleType.Script, bundledParts));
            }

            if (nonBundledParts.Any())
            {
                foreach (var path in nonBundledParts)
                {
					sb.AppendFormat("<script src=\"{0}\" type=\"text/javascript\"></script>", Scripts.Url(TryFindMinFile(path)).ToString());
                    sb.Append(Environment.NewLine);
                }
            }

            BundleTable.EnableOptimizations = prevEnableOptimizations;

            return sb.ToString();
        }

		private string TryFindMinFile(string path)
		{
			if (_httpContext.IsDebuggingEnabled)
			{
				// return path as is in debug mode
				return path;
			}

			return s_minFiles.GetOrAdd(path, key => 
			{
				try
				{
					if (!_httpContext.Request.IsUrlLocalToHost(key))
					{
						// no need to look for external files
						return key;
					}

					if (BundleTable.Bundles.GetBundleFor(key) != null)
					{
						// no need to seek min files for real bundles.
						return key;
					}

					var extension = Path.GetExtension(key);
					if (key.EndsWith(".min" + extension, StringComparison.InvariantCultureIgnoreCase))
					{
						// is already a debug file, get out!
						return key;
					}

					var minPath = "{0}.min{1}".FormatInvariant(key.Substring(0, key.Length - extension.Length), extension);
					if (HostingEnvironment.VirtualPathProvider.FileExists(minPath))
					{
						return minPath;
					}
					return key;
				}
				catch 
				{
					return key;
				}
			});
		}


        public void AddCssFileParts(ResourceLocation location, IEnumerable<string> parts, bool excludeFromBundling = false, bool append = false)
        {
            if (!_cssParts.ContainsKey(location))
                _cssParts.Add(location, new List<WebAssetDescriptor>());

            var descriptors = parts.Select(x => new WebAssetDescriptor
            {
				ExcludeFromBundling = excludeFromBundling || !x.StartsWith("~") || BundleTable.Bundles.GetBundleFor(x) != null,
                Part = x
            });

            AddPartsCore(_cssParts[location], descriptors, append);
        }

        public string GenerateCssFiles(UrlHelper urlHelper, ResourceLocation location, bool? enableBundling = null)
        {
            if (!_cssParts.ContainsKey(location) || _cssParts[location] == null)
                return "";

            if (_cssParts[location].Count == 0)
                return "";

            if (!enableBundling.HasValue)
            {
                enableBundling = this.BundlingEnabled;
            }

            var prevEnableOptimizations = BundleTable.EnableOptimizations;
            BundleTable.EnableOptimizations = enableBundling.Value;

            var parts = _cssParts[location];

            var bundledParts = parts.Where(x => !x.ExcludeFromBundling).Select(x => x.Part).Distinct();
            var nonBundledParts = parts.Where(x => x.ExcludeFromBundling).Select(x => x.Part).Distinct();

            var sb = new StringBuilder();

            if (bundledParts.Any())
            {
                sb.AppendLine(_bundleBuilder.Build(BundleType.Stylesheet, bundledParts));
            }

            if (nonBundledParts.Any())
            {
                foreach (var path in nonBundledParts)
                {
					sb.AppendFormat("<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />", Styles.Url(TryFindMinFile(path)).ToString());
                    sb.Append(Environment.NewLine);
                }
            }

            BundleTable.EnableOptimizations = prevEnableOptimizations;

            return sb.ToString();
        }

        private bool BundlingEnabled
        {
            get
            {
                if (_themeSettings.BundleOptimizationEnabled > 0)
                {
                    return _themeSettings.BundleOptimizationEnabled == 2;
                }

                return !HttpContext.Current.IsDebuggingEnabled;
            }
        }

		public void AddLinkPart(string rel, string href, RouteValueDictionary htmlAttributes)
		{
			Guard.NotEmpty(rel, nameof(rel));
			Guard.NotEmpty(href, nameof(href));

			if (htmlAttributes == null)
			{
				htmlAttributes = new RouteValueDictionary();
			}

			htmlAttributes["rel"] = rel;
			htmlAttributes["href"] = href;

			_linkParts.Add(htmlAttributes);
		}

		public string GenerateLinkRels()
		{
			var sb = new StringBuilder();
			
			foreach (var part in _linkParts)
			{
				var tag = new TagBuilder("link");
				tag.MergeAttributes(part, true);

				sb.AppendLine(tag.ToString(TagRenderMode.SelfClosing));
			}

			return sb.ToString();
		}

		public string GenerateMetaRobots()
		{
			if (_seoSettings.MetaRobotsContent.HasValue())
			{
				return "<meta name=\"robots\" content=\"{0}\" />".FormatInvariant(_seoSettings.MetaRobotsContent);
			}
			return null;
		}

		public class WebAssetDescriptor
        {
            public bool ExcludeFromBundling { get; set; }
            public string Part { get; set; }
        }

	}
}
