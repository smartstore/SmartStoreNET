﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Data.Setup;

namespace SmartStore.Web.Infrastructure.Installation
{
    /// <summary>
    /// Localization service for installation process
    /// </summary>
    public partial class InstallationLocalizationService : IInstallationLocalizationService
    {
        private const string LanguageCookieName = "sm.installation.lang";

        private IList<InstallationLanguage> _availableLanguages;
        private IEnumerable<Lazy<InvariantSeedData, InstallationAppLanguageMetadata>> _seedDatas;

		public InstallationLocalizationService(IEnumerable<Lazy<InvariantSeedData, InstallationAppLanguageMetadata>> seedDatas)
        {
            this._seedDatas = seedDatas;
        }

        public string GetResource(string resourceName)
        {
            var language = GetCurrentLanguage();
            if (language == null)
                return resourceName;
            var resourceValue = language.Resources
                .Where(r => r.Name.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase))
                .Select(r => r.Value)
                .FirstOrDefault();
            if (String.IsNullOrEmpty(resourceValue))
                // return name
                return resourceName;

            return resourceValue;
        }

        public virtual InstallationLanguage GetCurrentLanguage()
        {
           var httpContext = EngineContext.Current.Resolve<HttpContextBase>();
            
            var cookieLanguageCode = "";
            var cookie = httpContext.Request.Cookies[LanguageCookieName];
            if (cookie != null && !String.IsNullOrEmpty(cookie.Value))
                cookieLanguageCode = cookie.Value;

            // ensure it's available (it could be deleted since the previous installation)
            var availableLanguages = GetAvailableLanguages();

            var language = availableLanguages
                .Where(l => l.Code.Equals(cookieLanguageCode, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
            if (language != null)
                return language;

			// try to resolve install language from CurrentCulture
			language = availableLanguages.Where(MatchLanguageByCurrentCulture).FirstOrDefault();
			if (language != null)
				return language;

            // if we got here, the language code is not found. let's return the default one
            language = availableLanguages.Where(l => l.IsDefault).FirstOrDefault();
            if (language != null)
                return language;

            // return any available language
            language = availableLanguages.FirstOrDefault();
            return language;
        }

		private bool MatchLanguageByCurrentCulture(InstallationLanguage language)
		{
			var curCulture = Thread.CurrentThread.CurrentUICulture;

			if (language.Code.IsCaseInsensitiveEqual(curCulture.IetfLanguageTag))
				return true;

			curCulture = curCulture.Parent;
			if (curCulture != null)
			{
				return language.Code.IsCaseInsensitiveEqual(curCulture.IetfLanguageTag);
			}

			return false;
		}

        public virtual void SaveCurrentLanguage(string languageCode)
        {
            var httpContext = EngineContext.Current.Resolve<HttpContextBase>();

            var cookie = new HttpCookie(LanguageCookieName);
			cookie.HttpOnly = true;
            cookie.Value = languageCode;
            cookie.Expires = DateTime.Now.AddHours(24);

            httpContext.Response.Cookies.Remove(LanguageCookieName);
            httpContext.Response.Cookies.Add(cookie);
        }

        public virtual IEnumerable<InstallationAppLanguageMetadata> GetAvailableAppLanguages()
        {
            foreach (var data in _seedDatas)
            {
                yield return data.Metadata;
            }
        }

        public Lazy<InvariantSeedData, InstallationAppLanguageMetadata> GetAppLanguage(string culture)
        {
            Guard.ArgumentNotEmpty(culture, "culture");
            return _seedDatas.FirstOrDefault(x => x.Metadata.Culture == culture);
        }

        public virtual IList<InstallationLanguage> GetAvailableLanguages()
        {
            if (_availableLanguages == null)
            {
                _availableLanguages = new List<InstallationLanguage>();
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                foreach (var filePath in Directory.EnumerateFiles(webHelper.MapPath("~/App_Data/Localization/Installation/"), "*.xml"))
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(filePath);


                    //get language code
                    var languageCode = "";
                    //we file name format: installation.{languagecode}.xml
                    var r = new Regex(Regex.Escape("installation.") + "(.*?)" + Regex.Escape(".xml"));
                    var matches = r.Matches(Path.GetFileName(filePath));
                    foreach (Match match in matches)
                        languageCode = match.Groups[1].Value;

                    //get language friendly name
                    var languageName = xmlDocument.SelectSingleNode(@"//Language").Attributes["Name"].InnerText.Trim();
                    
                    //is default
                    var isDefaultAttribute = xmlDocument.SelectSingleNode(@"//Language").Attributes["IsDefault"];
                    var isDefault = isDefaultAttribute != null ? Convert.ToBoolean(isDefaultAttribute.InnerText.Trim()) : false;

                    //is default
                    var isRightToLeftAttribute = xmlDocument.SelectSingleNode(@"//Language").Attributes["IsRightToLeft"];
                    var isRightToLeft = isRightToLeftAttribute != null ? Convert.ToBoolean(isRightToLeftAttribute.InnerText.Trim()) : false;

                    //create language
                    var language = new InstallationLanguage
                    {
                        Code = languageCode,
                        Name = languageName,
                        IsDefault = isDefault,
                        IsRightToLeft = isRightToLeft,
                    };
                    //load resources
                    foreach (XmlNode resNode in xmlDocument.SelectNodes(@"//Language/LocaleResource"))
                    {
                        var resNameAttribute = resNode.Attributes["Name"];
                        var resValueNode = resNode.SelectSingleNode("Value");

                        if (resNameAttribute == null)
                            throw new SmartException("All installation resources must have an attribute Name=\"Value\".");
                        var resourceName = resNameAttribute.Value.Trim();
                        if (string.IsNullOrEmpty(resourceName))
                            throw new SmartException("All installation resource attributes 'Name' must have a value.'");

                        if (resValueNode == null)
                            throw new SmartException("All installation resources must have an element \"Value\".");
                        var resourceValue = resValueNode.InnerText.Trim();
                        
                        language.Resources.Add(new InstallationLocaleResource()
                        {
                            Name = resourceName,
                            Value = resourceValue
                        });
                    }

                    _availableLanguages.Add(language);
                    _availableLanguages = _availableLanguages.OrderBy(l => l.Name).ToList();

                }
            }
            return _availableLanguages;
        }

    }
}
