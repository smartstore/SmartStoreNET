using System;
using System.IO;
using System.Web;
using DotLiquid;
using DotLiquid.FileSystems;
using DotLiquid.NamingConventions;
using SmartStore.Core;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.IO;
using SmartStore.Core.Localization;
using SmartStore.Core.Themes;
using SmartStore.Services;

namespace SmartStore.Templating.Liquid
{
    public partial class LiquidTemplateEngine : ITemplateEngine, ITemplateFileSystem
    {
        private readonly Work<ICommonServices> _services;
        private readonly Work<LocalizerEx> _localizer;
        private readonly Work<IThemeContext> _themeContext;
        private readonly IVirtualPathProvider _vpp;

        public LiquidTemplateEngine(
            Work<ICommonServices> services,
            IVirtualPathProvider vpp,
            Work<IThemeContext> themeContext,
            Work<LocalizerEx> localizer)
        {
            _services = services;
            _vpp = vpp;
            _localizer = localizer;
            _themeContext = themeContext;

            // Register Value type transformers
            var allowedMoneyProps = new[]
            {
                nameof(Money.Amount),
                nameof(Money.RoundedAmount),
                nameof(Money.TruncatedAmount),
                nameof(Money.Formatted),
                nameof(Money.DecimalDigits)
            };
            Template.RegisterSafeType(typeof(Money), allowedMoneyProps, x => x);

            // Register tag "zone"
            Template.RegisterTagFactory(new ZoneTagFactory(_services.Value.EventPublisher));

            // Register Filters
            Template.RegisterFilter(typeof(AdditionalFilters));

            Template.NamingConvention = new CSharpNamingConvention();
            Template.FileSystem = this;
        }

        #region Services

        public ICommonServices Services => _services.Value;

        public LocalizedString T(string key, int languageId, params object[] args)
        {
            return _localizer.Value(key, languageId, args);
        }

        #endregion

        #region ITemplateEngine

        public ITemplate Compile(string source)
        {
            Guard.NotNull(source, nameof(source));

            return new LiquidTemplate(Template.Parse(source), source);
        }

        public string Render(string source, object model, IFormatProvider formatProvider)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(model, nameof(model));
            Guard.NotNull(formatProvider, nameof(formatProvider));

            return Compile(source).Render(model, formatProvider);
        }

        public ITestModel CreateTestModelFor(BaseEntity entity, string modelPrefix)
        {
            Guard.NotNull(entity, nameof(entity));

            return new TestDrop(entity, modelPrefix);
        }

        #endregion

        #region ITemplateFileSystem

        public Template GetTemplate(Context context, string templateName)
        {
            var virtualPath = ResolveVirtualPath(context, templateName);

            if (virtualPath.IsEmpty())
            {
                return null;
            }

            var cacheKey = HttpRuntime.Cache.BuildScopedKey("LiquidPartial://" + virtualPath);
            var cachedTemplate = HttpRuntime.Cache.Get(cacheKey);

            if (cachedTemplate == null)
            {
                // Read from file, compile and put to cache with file dependeny
                var source = ReadTemplateFileInternal(virtualPath);
                cachedTemplate = Template.Parse(source);
                var cacheDependency = _vpp.GetCacheDependency(virtualPath, DateTime.UtcNow);
                HttpRuntime.Cache.Insert(cacheKey, cachedTemplate, cacheDependency);
            }

            return (Template)cachedTemplate;
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            var virtualPath = ResolveVirtualPath(context, templateName);

            return ReadTemplateFileInternal(virtualPath);
        }

        private string ReadTemplateFileInternal(string virtualPath)
        {
            if (virtualPath.IsEmpty())
            {
                return string.Empty;
            }

            if (!_vpp.FileExists(virtualPath))
            {
                throw new FileNotFoundException($"Include file '{virtualPath}' does not exist.");
            }

            using (var stream = _vpp.OpenFile(virtualPath))
            {
                return stream.AsString();
            }
        }

        private string ResolveVirtualPath(Context context, string templateName)
        {
            var path = ((string)context[templateName]).NullEmpty() ?? templateName;

            if (path.IsEmpty())
                return string.Empty;

            path = path.EnsureEndsWith(".liquid");

            string virtualPath = null;

            if (!path.StartsWith("~/"))
            {
                var currentTheme = _themeContext.Value.CurrentTheme;
                virtualPath = _vpp.Combine(currentTheme.Location, currentTheme.ThemeName, "Views/Shared/EmailTemplates", path);
            }
            else
            {
                virtualPath = VirtualPathUtility.ToAppRelative(path);
            }
            return virtualPath;
        }

        #endregion
    }
}
