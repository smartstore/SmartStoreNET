using System;

namespace SmartStore.Core.Themes
{
	public class ThemeTouchedEvent
	{
		public ThemeTouchedEvent(string themeName)
		{
			Guard.ArgumentNotEmpty(() => themeName);
			this.ThemeName = themeName;
		}
		
		public string ThemeName { get; set; }
	}
}
