using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Directory
{
    public static class CurrencyExtensions
    {
        public static bool HasDomainEnding(this Currency currency, string domain)
        {
            if (currency == null || domain.IsEmpty() || currency.DomainEndings.IsEmpty())
                return false;

            var endings = currency.DomainEndings.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return endings.Any(x => domain.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
        }

        public static Currency GetByDomainEnding(this IEnumerable<Currency> currencies, string domain)
        {
            if (currencies == null || domain.IsEmpty())
                return null;

            return currencies.FirstOrDefault(x => x.Published && x.HasDomainEnding(domain));
        }
    }
}
