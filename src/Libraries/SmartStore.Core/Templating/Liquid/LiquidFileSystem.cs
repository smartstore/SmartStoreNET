using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using DotLiquid;
using DotLiquid.FileSystems;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;
using SmartStore.Core.Themes;

namespace SmartStore.Templating.Liquid
{
	internal class LiquidFileSystem : ITemplateFileSystem
	{
		private readonly IVirtualPathProvider _vpp;

		public LiquidFileSystem(IVirtualPathProvider vpp)
		{
			_vpp = vpp;
		}

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
				var currentTheme = EngineContext.Current.Resolve<IThemeContext>().CurrentTheme;
				virtualPath = _vpp.Combine(currentTheme.Location, currentTheme.ThemeName, "Views/Shared/EmailTemplates", path);
			}
			else
			{
				virtualPath = VirtualPathUtility.ToAppRelative(path);
			}

			return virtualPath;
		}
	}
}
