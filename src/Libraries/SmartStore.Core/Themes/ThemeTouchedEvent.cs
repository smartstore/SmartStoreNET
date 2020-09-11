namespace SmartStore.Core.Themes
{
    public class ThemeTouchedEvent
    {
        public ThemeTouchedEvent(string themeName)
        {
            Guard.NotEmpty(themeName, nameof(themeName));
            this.ThemeName = themeName;
        }

        public string ThemeName { get; set; }
    }
}
