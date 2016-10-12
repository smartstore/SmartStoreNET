using System;
using System.Net;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmDir = SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Directory
{
    public partial class GeoCountryLookup : IGeoCountryLookup
    {
        private readonly IWebHelper _webHelper;
		private readonly ICountryService _countryService;
		private readonly IRequestCache _requestCache;
		private readonly ICacheManager _cache;

		public GeoCountryLookup(IWebHelper webHelper, IRequestCache requestCache, ICacheManager cache, ICountryService countryService)
        {
            this._webHelper = webHelper;
			this._requestCache = requestCache;
			this._cache = cache;
			this._countryService = countryService;
        }

		private MaxMind.GeoIP.LookupService GetLookupService() 
		{
			return _cache.Get("GeoCountryLookup", () => 
			{
				var lookupService = new MaxMind.GeoIP.LookupService(_webHelper.MapPath("~/App_Data/GeoIP.dat"));
				return lookupService;
			});
		}

        public virtual string LookupCountryCode(string str)
        {
            if (String.IsNullOrEmpty(str))
                return string.Empty;

            IPAddress addr;
            try
            {
                addr = IPAddress.Parse(str);
            }
            catch
            {
                return string.Empty;
            }
            return LookupCountryCode(addr);
        }

        public virtual string LookupCountryCode(IPAddress addr)
        {
			try
			{
				var lookupService = GetLookupService();
				var country = lookupService.getCountry(addr);
				var code = country.getCode();
				if (code == "--")
					return string.Empty;

				return code;
			}
			catch 
			{
				return string.Empty;
			}
        }

        public virtual string LookupCountryName(string str)
        {
            if (String.IsNullOrEmpty(str))
                return string.Empty;

            IPAddress addr;
            try
            {
                addr = IPAddress.Parse(str);
            }
            catch
            {
                return string.Empty;
            }
            return LookupCountryName(addr);
        }

        public virtual string LookupCountryName(IPAddress addr)
        {
			try
			{
				var lookupService = GetLookupService();
				var country = lookupService.getCountry(addr);
				return country.getName();
			}
			catch
			{
				return string.Empty;
			}
        }

		public virtual bool IsEuIpAddress(string ipAddress, out SmDir.Country euCountry)
		{
			euCountry = null;

			if (ipAddress.IsEmpty())
				return false;

			euCountry = _requestCache.Get("GeoCountryLookup.EuCountry.{0}".FormatInvariant(ipAddress), () => 
			{
				var countryCode = LookupCountryCode(ipAddress);
				if (countryCode.IsEmpty())
					return (SmDir.Country)null;

				var country = _countryService.GetCountryByTwoLetterIsoCode(countryCode);
				return country;
			});

			if (euCountry == null)
				return false;

			return euCountry.SubjectToVat;
		}

    }
}