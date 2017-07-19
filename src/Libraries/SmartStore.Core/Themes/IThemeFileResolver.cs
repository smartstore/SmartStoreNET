using System;

namespace SmartStore.Core.Themes
{
	public interface IThemeFileResolver
	{
		InheritedThemeFileResult Resolve(string virtualPath);
	}
}
