using System;

namespace SmartStore.Web.Framework.Events
{
	public class ThemeTouched
	{
		public ThemeTouched(string themeName)
		{
			Guard.ArgumentNotEmpty(() => themeName);
			this.ThemeName = themeName;
		}
		
		public string ThemeName { get; set; }
	}
}
