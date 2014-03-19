using System.Collections.Generic;

namespace SmartStore.Core.Events
{

	public interface IConsumerFactory<T>
	{
		/// <summary>
		/// Resolves all event consumers of type <typeparamref name="T"/>
		/// </summary>
		/// <param name="resolveAsyncs">
		/// Specifies whether only async consumers, sync consumers or both should be resolved
		/// (<c>null</c> = all, <c>true</c> = only async, <c>false</c> = only sync)
		/// </param>
		/// <returns>The consumer instances</returns>
		IEnumerable<IConsumer<T>> GetConsumers(bool? resolveAsyncs = null);

		bool HasAsyncConsumer { get; }
	}

}
