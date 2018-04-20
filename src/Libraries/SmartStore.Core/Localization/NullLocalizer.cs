using System;

namespace SmartStore.Core.Localization
{
	public static class NullLocalizer
	{
		private static readonly Localizer _instance;
		private static readonly LocalizerEx _instanceEx;

		static NullLocalizer()
		{
			_instance = (format, args) => new LocalizedString((args == null || args.Length == 0) ? format : string.Format(format, args));
			_instanceEx = (format, languageId, args) => new LocalizedString((args == null || args.Length == 0) ? format : string.Format(format, args));
		}	

		public static Localizer Instance 
		{ 
			get { return _instance; } 
		}

		public static LocalizerEx InstanceEx
		{
			get { return _instanceEx; }
		}
	}
}
