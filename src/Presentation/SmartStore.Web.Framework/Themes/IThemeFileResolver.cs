using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace SmartStore.Web.Framework.Themes
{

	public interface IThemeFileResolver
	{
		/// <summary>
		/// Tries to resolve a file up in the current theme's hierarchy chain.
		/// </summary>
		/// <param name="virtualPath">The original virtual path of the theme file</param>
		/// <returns>
		/// If the current working themme is based on another theme AND the requested file
		/// was physically found in the theme's hierarchy chain, an instance of <see cref="InheritedThemeFileResult" /> will be returned.
		/// In any other case the return value is <c>null</c>.
		/// </returns>
		InheritedThemeFileResult Resolve(string virtualPath);
	}

	public class InheritedThemeFileResult
	{
		/// <summary>
		/// The unrooted relative path of the file (without <c>~/Themes/ThemeName/</c>)
		/// </summary>
		public string RelativePath { get; set; }

		/// <summary>
		/// The original virtual path
		/// </summary>
		public string OriginalVirtualPath { get; set; }

		/// <summary>
		/// The result virtual path (the path in which the file is actually located)
		/// </summary>
		public string ResultVirtualPath { get; set; }

		/// <summary>
		/// The result physical path (the path in which the file is actually located)
		/// </summary>
		public string ResultPhysicalPath { get; set; }

		/// <summary>
		/// The name of the requesting theme
		/// </summary>
		public string OriginalThemeName { get; set; }

		/// <summary>
		/// The name of the resulting theme where the file is actually located
		/// </summary>
		public string ResultThemeName { get; set; }
	}

}
