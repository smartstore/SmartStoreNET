using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SmartStore.Core.IO;
using SmartStore.Core.Themes;
using DotLiquid;
using SmartStore.Core.Infrastructure;
using System.Web;

namespace SmartStore.Templating.Liquid
{
	internal class LiquidFileSystem : DotLiquid.FileSystems.IFileSystem
	{
		private readonly IVirtualPathProvider _vpp;

		public LiquidFileSystem(IVirtualPathProvider vpp)
		{
			_vpp = vpp;
		}

		public string ReadTemplateFile(Context context, string templateName)
		{
			var path = ((string)context[templateName]).NullEmpty() ?? templateName;

			if (path.IsEmpty())
				return string.Empty;

			path = path.EnsureEndsWith(".liquid");

			var virtualPath = ResolveVirtualPath(path);

			if (!_vpp.FileExists(virtualPath))
			{
				throw new IOException($"Include file '{virtualPath}' does not exist.");
			}

			using (var stream = _vpp.OpenFile(virtualPath))
			{
				return stream.AsString();
			}
		}

		private string ResolveVirtualPath(string path)
		{
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
