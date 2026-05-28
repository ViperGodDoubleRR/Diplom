using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shared.RabbitMQ.EventBus.Abstractions;

namespace Shared.RabbitMQ.EventBus.Subscriptions
{
    public class SubscriptionManager
    {
        private readonly Dictionary<string, List<Type>> _handlers = new();
        private readonly Dictionary<string, Type> _eventTypes = new();

        public void AddSubscription<TEvent, THandler>()
            where TEvent : IIntegrationEvent
            where THandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;

            _eventTypes[eventName] = typeof(TEvent);

            if (!_handlers.ContainsKey(eventName))
                _handlers[eventName] = new List<Type>();

            _handlers[eventName].Add(typeof(THandler));
        }

        public IEnumerable<string> GetEventNames() => _handlers.Keys;

        public Type? GetEventType(string eventName)
            => _eventTypes.TryGetValue(eventName, out var t) ? t : null;

        public List<Type> GetHandlers(string eventName)
            => _handlers.TryGetValue(eventName, out var list)
                ? list
                : new List<Type>();
    }
}
