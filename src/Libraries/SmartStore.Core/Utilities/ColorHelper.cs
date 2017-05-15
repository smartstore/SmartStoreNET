using System;
using System.Drawing;

namespace SmartStore.Utilities
{
	public static class ColorHelper
	{
		public static int GetPerceivedBrightness(string htmlColor)
		{
            if (String.IsNullOrEmpty(htmlColor))
                htmlColor = "#ffffff";
            
            return GetPerceivedBrightness(ColorTranslator.FromHtml(htmlColor));
		}

		/// <summary>
		/// Calculates the perceived brightness of a color.
		/// </summary>
		/// <param name="color">The color</param>
		/// <returns>
		/// A number in the range of 0 (black) to 255 (White). 
		/// For text contrast colors, an optimal cutoff value is 130.
		/// </returns>
		public static int GetPerceivedBrightness(Color color)
		{
			return (int)Math.Sqrt(
			   color.R * color.R * .241 +
			   color.G * color.G * .691 +
			   color.B * color.B * .068);
		}
	}
}
