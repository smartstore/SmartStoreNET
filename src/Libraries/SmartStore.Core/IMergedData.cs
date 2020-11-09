using System.Collections.Generic;
using System.Web.Hosting;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core
{
    public interface IMergedData
    {
        bool MergedDataIgnore { get; set; }
        Dictionary<string, object> MergedDataValues { get; }
    }

    public static class IMergedDataExtensions
    {
        public static T GetMergedDataValue<T>(this IMergedData mergedData, string key, T defaultValue)
        {
            if (mergedData.MergedDataValues == null)
                return defaultValue;

            if (mergedData.MergedDataIgnore)
                return defaultValue;

            if (mergedData is BaseEntity && HostingEnvironment.IsHosted)
            {
                // This is absolutely bad coding! But I don't see any alternatives.
                // When the passed object is a (EF)-trackable entity,
                // we cannot return the merged value while EF performs
                // change detection, because entity properties could be set to modified,
                // where in fact nothing has changed.
                var dbContext = EngineContext.Current.Resolve<IDbContext>();
                if (dbContext.IsDetectingChanges())
                {
                    return defaultValue;
                }
            }

            object value;
            if (mergedData.MergedDataValues.TryGetValue(key, out value))
            {
                return (T)value;
            }

            return defaultValue;
        }
    }
}
