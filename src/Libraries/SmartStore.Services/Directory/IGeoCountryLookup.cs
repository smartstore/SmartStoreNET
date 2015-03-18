using System.Net;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// Country lookup helper
    /// </summary>
    public partial interface IGeoCountryLookup
    {
        string LookupCountryCode(string str);

        string LookupCountryCode(IPAddress addr);

        string LookupCountryName(string str);

        string LookupCountryName(IPAddress addr);

		/// <summary>
		/// Gets a value indicating whether the given IP address originates from an EU country
		/// </summary>
		/// <param name="ipAddress">IP address</param>
		/// <param name="euCountry">An instance of <see cref="Country"/> if the IP originates from a EU country</param>
		/// <returns>
		/// <c>true</c> if the IP address originates from an EU country, <c>false</c> if not
		/// </returns>
		bool IsEuIpAddress(string ipAddress, out Country euCountry);
    }
}