using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Subscriptions;
using Shared.RabbitMQ.RabbitMq;
namespace Shared.RabbitMQ.EventBus.RabbitMQ
{
    public class RabbitMqEventBus : IEventBus
    {
        private const string ExchangeName = "social";

        private readonly IServiceProvider _sp;
        private readonly RabbitMqConnection _conn;
        private readonly SubscriptionManager _subscriptions;

        private IChannel? _publishChannel;

        public RabbitMqEventBus(
            IServiceProvider sp,
            RabbitMqConnection conn,
            SubscriptionManager subscriptions)
        {
            _sp = sp;
            _conn = conn;
            _subscriptions = subscriptions;
        }

        // ================= INIT =================
        public async Task InitAsync()
        {
            _publishChannel = await _conn.CreateChannelAsync();

            await _publishChannel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);
        }

        // ================= SUBSCRIBE =================
        public void Subscribe<TEvent, THandler>()
            where TEvent : IIntegrationEvent
            where THandler : IEventHandler<TEvent>
        {
            _subscriptions.AddSubscription<TEvent, THandler>();
        }

        // ================= PUBLISH =================
        public async Task Publish<TEvent>(TEvent @event)
            where TEvent : IIntegrationEvent
        {
            if (_publishChannel == null)
                throw new Exception("Call InitAsync first");

            var json = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(json);

            await _publishChannel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: typeof(TEvent).Name,
                mandatory: false,
                basicProperties: new BasicProperties
                {
                    Persistent = true
                },
                body: body);
        }

        // ================= CONSUMER =================
        public async Task StartConsumingAsync(string queueName)
        {
            var channel = await _conn.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                ExchangeName,
                ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            foreach (var eventName in _subscriptions.GetEventNames())
            {
                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: ExchangeName,
                    routingKey: eventName);
            }

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                var eventName = ea.RoutingKey;

                var eventType = _subscriptions.GetEventType(eventName);
                if (eventType == null)
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize(json, eventType);

                using var scope = _sp.CreateScope();

                var handlers = _subscriptions.GetHandlers(eventName);

                foreach (var handlerType in handlers)
                {
                    var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                    var method = handlerType.GetMethod("Handle")!;

                    await (Task)method.Invoke(handler, new[] { message })!;
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }
    }
}
