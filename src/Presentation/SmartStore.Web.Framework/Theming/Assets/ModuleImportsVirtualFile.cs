using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Web.Hosting;
using SmartStore.Core.Plugins;
using SmartStore.Core.Data;

namespace SmartStore.Web.Framework.Theming.Assets
{
	public class ModuleImportsVirtualFile : VirtualFile
	{
		private static readonly HashSet<string> _adminImports;
		private static readonly HashSet<string> _publicImports;

		static ModuleImportsVirtualFile()
		{
			_adminImports = new HashSet<string>();
			_publicImports = new HashSet<string>();

			if (DataSettings.DatabaseIsInstalled())
			{
				CollectModuleImports();
			}	
		}

		private static void CollectModuleImports()
		{
			var installedPlugins = PluginManager.ReferencedPlugins.Where(x => x.Installed);
			var root = PluginManager.PluginsLocation;

			foreach (var plugin in installedPlugins)
			{
				var contentDir = Path.Combine(plugin.PhysicalPath, "Content");
				if (!Directory.Exists(contentDir))
					continue;

				if (File.Exists(Path.Combine(contentDir, "public.scss")))
				{
					_publicImports.Add($"{root}/{plugin.FolderName}/Content/public.scss");
				}

				if (File.Exists(Path.Combine(contentDir, "admin.scss")))
				{
					_adminImports.Add($"{root}/{plugin.FolderName}/Content/admin.scss");
				}
			}
		}

		private readonly bool _isAdmin;

		public ModuleImportsVirtualFile(string virtualPath, bool isAdmin)
			: base(virtualPath)
		{
			_isAdmin = isAdmin;
		}

		public override bool IsDirectory
		{
			get { return false; }
		}

		public override Stream Open()
		{
			var sb = new StringBuilder();

			var imports = _isAdmin ? _adminImports : _publicImports;
			foreach (var imp in imports)
			{
				sb.AppendLine($"@import '{imp}';");
			}

			return GenerateStreamFromString(sb.ToString());
		}

		private Stream GenerateStreamFromString(string value)
		{
			var stream = new MemoryStream();

			using (var writer = new StreamWriter(stream, Encoding.Unicode, 1024, true))
			{
				writer.Write(value);
				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
		}

	}
}