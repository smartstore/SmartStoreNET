using System;
using System.IO;
using System.Web.Hosting;
using SmartStore.Core.Themes;

namespace SmartStore.Web.Framework.Theming
{
    internal class InheritedVirtualThemeFile : VirtualFile
    {
		public InheritedVirtualThemeFile(InheritedThemeFileResult resolveResult)
			: base(DetermineVirtualPath(resolveResult))
        {
            ResolveResult = resolveResult;
        }

		public InheritedThemeFileResult ResolveResult { get; }

		public string ResultVirtualPath
		{
			get { return ResolveResult.ResultVirtualPath ?? ResolveResult.OriginalVirtualPath; }
		}

		public override Stream Open()
        {
			return new FileStream(ResolveResult.ResultPhysicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

		private static string DetermineVirtualPath(InheritedThemeFileResult resolveResult)
		{
			if (resolveResult.RelativePath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
			{
				// ASP.NET BuildManager requires the original path for razor views,
				// otherwise an exception is thrown
				return resolveResult.OriginalVirtualPath;
			}
			else
			{
				return resolveResult.ResultVirtualPath;
			}
		}

    }
}