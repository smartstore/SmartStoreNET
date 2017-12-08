namespace SmartStore.Core.Localization
{
    public delegate LocalizedString Localizer(string key, params object[] args);
	public delegate LocalizedString LocalizerEx(string key, int languageId, params object[] args);
}