using System;

namespace SmartStore.Core.Events
{
    /// <summary>
    /// Marker interface for classes that contain one or more
    /// event message handlers. A handler is a public instance method following some conventions.
    /// The name of the handler method must be one of:
    /// <list type="bullet">
    ///		<item>void Handle(TMessage msg)</item>
    ///		<item>void HandleEvent(TMessage msg)</item>
    ///		<item>void Consume(TMessage msg)</item>
    ///		<item>Task HandleAsync(TMessage msg)</item>
    ///		<item>Task HandleEventAsync(TMessage msg)</item>
    ///		<item>Task ConsumeAsync(TMessage msg)</item>
    /// </list>
    /// Alternatively, the TMessage param can be the generic <see cref="ConsumeContext{TMessage}"/> type.
    /// <para>
    /// The <see cref="IConsumerInvoker"/> decides how to call the method based on its signature:
    /// <list type="bullet">
    ///		<item><c>void</c> method are invoked synchronously.</item>
    ///		<item><see cref="Task"/> methods are invoked asynchronously and awaited.</item>
    ///		<item>The <see cref="FireForgetAttribute"/> makes the method run in the background without awaiting.</item>
    /// </list>
    /// </para>
    /// <para>
    /// You can declare additional dependency parameters in the handler method, e.g.:
    /// <code>
    ///		Task HandleEventAsync(SomeEvent message, IDbContext dbContext, ICacheManager cache, CancellationToken cancelToken)
    /// </code>
    /// Order does not matter. The invoker will auto-resolve appropriate instances and pass them to the method. But any unregistered
    /// dependency or a primitive type will result in an exception (except of <see cref="CancellationToken"/> which always resolves to 
    /// <see cref="SmartStore.Core.Async.AsyncRunner.AppShutdownCancellationToken">).
    /// </para>
    /// </summary>
    public interface IConsumer
    {
    }

    [Obsolete("Implement the non-generic 'IConsumer' interface instead.")]
    public interface IConsumer<T> : IConsumer
    {
        void HandleEvent(T message);
    }
}
