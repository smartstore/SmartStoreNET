using System;

namespace SmartStore.Core.Events
{
    /// <summary>
    /// Specifies whether an event consumer method should be invoked asynchronously without awaiting.
    /// This can be advantegous in long running processes, because the current request thread will not 'block'
    /// until method completion.
    /// </summary>
    /// <remarks>
    /// Use this with caution and only if you know what you are doing :-)
    /// A class containing a fire & forget consumer should NOT take dependencies on request scoped services, because task continuation
    /// happens on another thread and context gets lost. Pass required dependencies as method parameters instead, the consumer invoker
    /// will spawn a new private context for the unit of work and resolve dependencies from this context.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class FireForgetAttribute : Attribute
    {
    }
}
