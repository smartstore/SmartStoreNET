using System;

namespace SmartStore.Core.Logging
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(Type type);
        ILogger GetLogger(string name);
        void FlushAll();
    }

    public static class ILoggerFactoryExtensions
    {
        public static ILogger CreateLogger<T>(this ILoggerFactory factory) where T : class
        {
            return factory.GetLogger(typeof(T));
        }
    }
}
