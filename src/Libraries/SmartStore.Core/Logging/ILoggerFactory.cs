using System;
using System.Diagnostics;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Core.Logging
{
	public interface ILoggerFactory
	{
		ILogger CreateLogger(Type type);
		ILogger CreateLogger(string name);
	}

	public static class ILoggerFactoryExtensions
	{
		public static ILogger CreateLogger<T>(this ILoggerFactory factory) where T : class
		{
			return factory.CreateLogger(typeof(T));
		}
	}
}
