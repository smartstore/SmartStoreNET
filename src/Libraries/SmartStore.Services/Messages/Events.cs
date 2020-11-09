using System;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
    /// <summary>
    /// Published after the creation of a single message model part has been completed.
    /// </summary>
    /// <typeparam name="T">Type of source entity</typeparam>
    public class MessageModelPartCreatedEvent<T> where T : class
    {
        public MessageModelPartCreatedEvent(T source, dynamic part)
        {
            Source = source;
            Part = part;
        }

        /// <summary>
        /// The source object for which the model part has been created, e.g. a Product entity.
        /// </summary>
        public T Source { get; private set; }

        /// <summary>
        /// The resulting model part.
        /// </summary>
        public dynamic Part { get; private set; }
    }

    /// <summary>
    /// Published when a system mapper is missing for a particular model type (e.g. a custom entity in a plugin).
    /// Implementors should subscribe to this event in order to provide a corresponding dynamic model part.
    /// The result model should be assigned to the <see cref="Result"/> property. If this property
    /// is still <c>null</c>, the source is used as model part instead.
    /// </summary>
    public class MessageModelPartMappingEvent
    {
        public MessageModelPartMappingEvent(object source)
        {
            Source = source;
        }

        /// <summary>
        /// The source object for which a model part should be created.
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        /// The resulting model part.
        /// </summary>
        public dynamic Result { get; set; }

        /// <summary>
        /// The name of the model part. If <c>null</c> the source's type name is used.
        /// </summary>
        public string ModelPartName { get; set; }
    }

    /// <summary>
    /// Published after the message model has been completely created.
    /// </summary>
    public class MessageModelCreatedEvent
    {
        public MessageModelCreatedEvent(MessageContext messageContext, TemplateModel model)
        {
            MessageContext = messageContext;
            Model = model;
        }

        public MessageContext MessageContext { get; private set; }

        /// <summary>
        /// The result message model.
        /// </summary>
        public TemplateModel Model { get; private set; }
    }

    /// <summary>
    /// An event message which gets published just before a new instance
    /// of <see cref="QueuedEmail"/> is persisted to the database
    /// </summary>
    public class MessageQueuingEvent
    {
        public QueuedEmail QueuedEmail { get; set; }
        public MessageContext MessageContext { get; set; }
        public TemplateModel MessageModel { get; set; }
    }
}
