using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RabbitMQ.EventBus.Abstractions
{
    public interface IEventBus
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : IIntegrationEvent;
        Task Subscribe<TEvent, THandle>()
        where TEvent : IIntegrationEvent
        where THandle : IEventHandler<TEvent>;
    }

}

