using System;

namespace SmartStore.Services.Events
{
    public interface IMessageBroker
    {
        void Subscribe(string channel, Action<string, string> handler);
        void Publish(string channel, string message);
    }
}
