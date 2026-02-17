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

        public Dictionary<Type, List<Type>> Handlers { get; } = new Dictionary<Type, List<Type>>();
        public void AddSubscription<TEvent, THandle>()
            where TEvent : IIntegrationEvent
            where THandle : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandle);

            var eventExists = Handlers.ContainsKey(eventType);
            if (eventExists)
            {
                var handleExits = Handlers[eventType].Contains(handlerType);
                if (!handleExits)
                    Handlers[eventType].Add(handlerType);
            }
            else
            {
                Handlers.Add(eventType, new List<Type>());
                Handlers[eventType].Add(handlerType);
            }

        }
        public bool HasSubscriptionsForEvent<TEvent>()
            where TEvent : IIntegrationEvent
        {
            var eventType = typeof(TEvent);
            return Handlers.ContainsKey(eventType);
        }
        public List<Type> GetHandlersForEvent<TEvent>()
    where TEvent : IIntegrationEvent
        {
            var eventType = typeof(TEvent);

            if (Handlers.ContainsKey(eventType))
                return Handlers[eventType];

            return new List<Type>();
        }
        public Type GetEventTypeByName(string eventName)
        {
            return Handlers.Keys.FirstOrDefault(t => t.Name == eventName);
        }
        public void RemoveSubscription<TEvent, THandle>()
    where TEvent : IIntegrationEvent
    where THandle : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandle);

            if (!Handlers.ContainsKey(eventType))
                return;

            Handlers[eventType].Remove(handlerType);

            if (Handlers[eventType].Count == 0)
                Handlers.Remove(eventType);
        }
        public void Clear()
        {
            Handlers.Clear();
        }

    }

}
