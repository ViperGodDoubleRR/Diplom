using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

using RabbitMQ.Client;

using RabbitMQ.Client.Events;

namespace Shared.RabbitMQ.EventBus.Abstractions
{
    public interface IEventBus
    {
        Task InitAsync();

        void Subscribe<TEvent, THandler>()
            where TEvent : IIntegrationEvent
            where THandler : IEventHandler<TEvent>;

        Task Publish<TEvent>(TEvent @event)
            where TEvent : IIntegrationEvent;

        Task StartConsumingAsync(string queueName);
    }
}
