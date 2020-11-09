using System.Linq;

namespace SmartStore
{
    public static class MiscExtensions
    {
        public static bool IsEntityFrameworkProvider(this IQueryProvider provider)
        {
            return provider.GetType().FullName == "System.Data.Objects.ELinq.ObjectQueryProvider";
        }
    }
}
