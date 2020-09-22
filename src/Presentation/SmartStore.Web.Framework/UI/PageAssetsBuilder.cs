using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Themes;
using SmartStore.Services.Localization;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore.Web.Framework.UI
{
    public partial class PageAssetsBuilder : IPageAssetsBuilder
    {
        private readonly HttpContextBase _httpContext;
        private readonly SeoSettings _seoSettings;
        private readonly ThemeSettings _themeSettings;
        private readonly IBundleBuilder _bundleBuilder;

        private List<string> _titleParts;
        private List<string> _metaDescriptionParts;
        private List<string> _metaKeywordParts;
        private List<string> _canonicalUrlParts;
        private List<string> _customHeadParts;
        private List<RouteValueDictionary> _linkParts;
        private Dictionary<ResourceLocation, List<WebAssetDescriptor>> _scriptParts;
        private Dictionary<ResourceLocation, List<WebAssetDescriptor>> _cssParts;
        private string _htmlId;

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
            _bundleBuilder = bundleBuilder;

            var bodyHtmlId = storeContext.CurrentStore.HtmlBodyId;
            if (bodyHtmlId.HasValue())
            {
                BodyAttributes["id"] = bodyHtmlId;
            }
        }

        private bool IsValidPart<T>(T part)
        {
            bool isValid = part != null;
            if (isValid)
            {
                if (part is string str)
                {
                    isValid = str.HasValue();
                }
            }

            return isValid;
        }

        // Helper func: changes all following public funcs to remove code redundancy
        private void AddPartsCore<T>(ref List<T> list, IEnumerable<T> partsToAdd, bool prepend = false)
        {
            var parts = (partsToAdd ?? Enumerable.Empty<T>()).Where(IsValidPart);

            if (list == null)
            {
                list = new List<T>(parts);
            }
            else if (parts.Any())
            {
                if (prepend)
                {
                    // insertion of multiple parts at the beginning
                    // should keep order (and not vice-versa as it was originally)
                    list.InsertRange(0, parts);
                }
                else
                {
                    list.AddRange(parts);
                }
            }
        }

        public IDictionary<string, object> BodyAttributes { get; } = new RouteValueDictionary();

        public void AddBodyAttribute(string name, object value)
        {
            Guard.NotEmpty(name, nameof(name));

            BodyAttributes[name] = value;
        }

        public void AddBodyCssClass(string className)
        {
            if (className.IsEmpty())
                return;

            if (className.HasValue())
            {
                BodyAttributes.PrependCssClass(className);
            }
        }

        public void SetHtmlId(string htmlId)
        {
            _htmlId = htmlId;
        }

        public string GenerateHtmlId()
        {
            if (!_htmlId.HasValue())
                return null;

            return _htmlId;
        }

        public void AddTitleParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(ref _titleParts, parts, append);
        }

        public virtual string GenerateTitle(bool addDefaultTitle)
        {
            if (_titleParts == null)
                return string.Empty;

            var result = string.Empty;
            var specificTitle = string.Join(_seoSettings.PageTitleSeparator, _titleParts.AsEnumerable().Reverse().ToArray());
            if (specificTitle.HasValue())
            {
                if (addDefaultTitle)
                {
                    // Store name + page title
                    switch (_seoSettings.PageTitleSeoAdjustment)
                    {
                        case PageTitleSeoAdjustment.PagenameAfterStorename:
                            {
                                result = string.Join(_seoSettings.PageTitleSeparator, _seoSettings.GetLocalizedSetting(x => x.MetaTitle).Value, specificTitle);
                            }
                            break;
                        case PageTitleSeoAdjustment.StorenameAfterPagename:
                        default:
                            {
                                result = string.Join(_seoSettings.PageTitleSeparator, specificTitle, _seoSettings.GetLocalizedSetting(x => x.MetaTitle).Value);
                            }
                            break;
                    }
                }
                else
                {
                    // Page title only
                    result = specificTitle;
                }
            }
            else
            {
                // Store name only
                result = _seoSettings.GetLocalizedSetting(x => x.MetaTitle).Value;
            }

            return result;
        }


        public void AddMetaDescriptionParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(ref _metaDescriptionParts, parts, append);
        }

        public virtual string GenerateMetaDescription()
        {
            var result = _seoSettings.GetLocalizedSetting(x => x.MetaDescription).Value;

            if (_metaDescriptionParts == null)
                return result;

            var metaDescription = string.Join(", ", _metaDescriptionParts.AsEnumerable().Reverse().ToArray());
            if (metaDescription.HasValue())
            {
                result = metaDescription;
            }

            return result;
        }


        public void AddMetaKeywordParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(ref _metaKeywordParts, parts, append);
        }

        public virtual string GenerateMetaKeywords()
        {
            var result = _seoSettings.GetLocalizedSetting(x => x.MetaKeywords).Value;

            if (_metaKeywordParts == null)
                return result;

            var metaKeyword = string.Join(", ", _metaKeywordParts.AsEnumerable().Reverse().ToArray());
            if (metaKeyword.HasValue())
            {
                result = metaKeyword;
            }

            return result;
        }

        public void AddCanonicalUrlParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(ref _canonicalUrlParts, parts, append);
        }

        public string GenerateCanonicalUrls()
        {
            if (_canonicalUrlParts == null)
                return string.Empty;

            var result = PooledStringBuilder.Rent();
            var parts = _canonicalUrlParts.Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var part in parts)
            {
                result.AppendFormat("<link rel=\"canonical\" href=\"{0}\" />", part);
                result.AppendLine();
            }

            return result.ToStringAndReturn();
        }

        public void AddCustomHeadParts(IEnumerable<string> parts, bool append = false)
        {
            AddPartsCore(ref _customHeadParts, parts, append);
        }

        public string GenerateCustomHead()
        {
            if (_customHeadParts == null)
                return string.Empty;

            var result = PooledStringBuilder.Rent();
            var parts = _customHeadParts.Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var part in parts)
            {
                result.AppendLine(part);
            }

            return result.ToStringAndReturn();
        }

        public void AddScriptParts(ResourceLocation location, IEnumerable<string> parts, bool excludeFromBundling = false, bool append = false)
        {
            if (_scriptParts == null)
            {
                _scriptParts = new Dictionary<ResourceLocation, List<WebAssetDescriptor>>();
            }

            if (!_scriptParts.TryGetValue(location, out var assetDescriptors))
            {
                assetDescriptors = new List<WebAssetDescriptor>();
                _scriptParts.Add(location, assetDescriptors);
            }

            var descriptors = parts.Select(x => new WebAssetDescriptor
            {
                ExcludeFromBundling = excludeFromBundling || !x.StartsWith("~") || BundleTable.Bundles.GetBundleFor(x) != null,
                Part = x
            });

            AddPartsCore(ref assetDescriptors, descriptors, append);
        }

        public string GenerateScripts(UrlHelper urlHelper, ResourceLocation location, bool? enableBundling = null)
        {
            if (_scriptParts == null || !_scriptParts.TryGetValue(location, out var parts) || parts == null || parts.Count == 0)
                return string.Empty;

            if (!enableBundling.HasValue)
            {
                enableBundling = this.BundlingEnabled;
            }

            var prevEnableOptimizations = BundleTable.EnableOptimizations;
            BundleTable.EnableOptimizations = enableBundling.Value;

            var bundledParts = parts.Where(x => !x.ExcludeFromBundling && x.Part.HasValue()).Select(x => x.Part).Distinct(StringComparer.OrdinalIgnoreCase);
            var nonBundledParts = parts.Where(x => x.ExcludeFromBundling && x.Part.HasValue()).Select(x => x.Part).Distinct(StringComparer.OrdinalIgnoreCase);

            var sb = PooledStringBuilder.Rent();

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

            return sb.ToStringAndReturn();
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
            if (_cssParts == null)
            {
                _cssParts = new Dictionary<ResourceLocation, List<WebAssetDescriptor>>();
            }

            if (!_cssParts.TryGetValue(location, out var assetDescriptors))
            {
                assetDescriptors = new List<WebAssetDescriptor>();
                _cssParts.Add(location, assetDescriptors);
            }

            var descriptors = parts.Select(x => new WebAssetDescriptor
            {
                ExcludeFromBundling = excludeFromBundling || !x.StartsWith("~") || BundleTable.Bundles.GetBundleFor(x) != null,
                Part = x
            });

            AddPartsCore(ref assetDescriptors, descriptors, append);
        }

        public string GenerateCssFiles(UrlHelper urlHelper, ResourceLocation location, bool? enableBundling = null)
        {
            if (_cssParts == null || !_cssParts.TryGetValue(location, out var parts) || parts == null || parts.Count == 0)
                return string.Empty;

            if (!enableBundling.HasValue)
            {
                enableBundling = this.BundlingEnabled;
            }

            var prevEnableOptimizations = BundleTable.EnableOptimizations;
            BundleTable.EnableOptimizations = enableBundling.Value;

            var bundledParts = parts.Where(x => !x.ExcludeFromBundling && x.Part.HasValue()).Select(x => x.Part).Distinct(StringComparer.OrdinalIgnoreCase);
            var nonBundledParts = parts.Where(x => x.ExcludeFromBundling && x.Part.HasValue()).Select(x => x.Part).Distinct(StringComparer.OrdinalIgnoreCase);

            var sb = PooledStringBuilder.Rent();

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

            return sb.ToStringAndReturn();
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

            if (_linkParts == null)
            {
                _linkParts = new List<RouteValueDictionary>();
            }

            _linkParts.Add(htmlAttributes);
        }

        public string GenerateLinkRels()
        {
            if (_linkParts == null)
                return string.Empty;

            var sb = PooledStringBuilder.Rent();

            foreach (var part in _linkParts)
            {
                var tag = new TagBuilder("link");
                tag.MergeAttributes(part, true);

                sb.AppendLine(tag.ToString(TagRenderMode.SelfClosing));
            }

            return sb.ToStringAndReturn();
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
