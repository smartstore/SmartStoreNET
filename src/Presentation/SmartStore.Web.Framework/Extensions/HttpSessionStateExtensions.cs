using System;
using System.Web;

namespace SmartStore
{
    public static class HttpSessionStateExtensions
    {
        public static void SafeRemove(this HttpSessionStateBase session, string key)
        {
            try
            {
                if (session != null && session[key] != null)
                    session.Remove(key);
            }
            catch (Exception exc)
            {
                exc.Dump();
            }
        }

        public static T SafeGetValue<T>(this HttpSessionStateBase session, string key, T defaultValue = default(T))
        {
            try
            {
                if (session != null && session[key] != null)
                    return (T)session[key];
            }
            catch (Exception exc)
            {
                exc.Dump();
            }

            return defaultValue;
        }

        public static void SafeSet(this HttpSessionStateBase session, string key, object value)
        {
            try
            {
                if (session != null)
                {
                    if (value == null)
                        session.SafeRemove(key);
                    else
                        session[key] = value;
                }
            }
            catch (Exception exc)
            {
                exc.Dump();
            }
        }
    }
}
