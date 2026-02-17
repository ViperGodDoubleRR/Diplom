using System;
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
    public class RabbitMqEventBus : IEventBus, IDisposable
    {
        private readonly RabbitMqConnectionPersistent _connection;
        private readonly SubscriptionManager _subscriptionManager;
        private readonly IServiceProvider _serviceProvider;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IChannel _listenerChannel;
        private Task? _listenerTask;

        private const string ExchangeName = "social";

        public RabbitMqEventBus(
            RabbitMqConnectionPersistent connection,
            SubscriptionManager subscriptionManager,
            IServiceProvider serviceProvider)
        {
            _connection = connection;
            _subscriptionManager = subscriptionManager;
            _serviceProvider = serviceProvider;

            _listenerChannel = _connection.CreateChannelAsync().Result; 

            StartListening();
        }


        public async Task Publish<TEvent>(TEvent @event) where TEvent : IIntegrationEvent
        {
            var channel = await _connection.CreateChannelAsync();

            string json = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties();

            await channel.BasicPublishAsync(
                ExchangeName,
                typeof(TEvent).Name,
                false,
                props,
                body
            );
        }

        public async Task Subscribe<TEvent, THandle>()
            where TEvent : IIntegrationEvent
            where THandle : IEventHandler<TEvent>
        {
            _subscriptionManager.AddSubscription<TEvent, THandle>();

            var eventName = typeof(TEvent).Name;
            var handlerName = typeof(THandle).Name;

            var queueName = eventName + "." + handlerName;

            var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queueName, true, false, false);
            await channel.QueueBindAsync(queueName, ExchangeName, eventName);
        }

        private void StartListening()
        {
            foreach (var kvp in _subscriptionManager.Handlers)
            {
                var eventType = kvp.Key;
                var handlers = kvp.Value;

                foreach (var handlerType in handlers)
                {
                    var queueName = eventType.Name + "." + handlerType.Name;
                    StartConsumer(queueName, eventType);
                }
            }
        }


        private void StartConsumer(string queueName, Type eventType)
        {
            var consumer = new AsyncEventingBasicConsumer(_listenerChannel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    await ProcessEvent(eventType, json);

                    await _listenerChannel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    await _listenerChannel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            _listenerChannel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumerTag: "",
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer
            );
        }



        private async Task ProcessEvent(Type eventType, string json)
        {
            var handlers = _subscriptionManager.Handlers[eventType];

            var integrationEvent = JsonSerializer.Deserialize(json, eventType)
                                  ?? throw new InvalidOperationException(
                                      $"Failed to deserialize event of type {eventType.Name}");

            foreach (var handlerType in handlers)
            {
                using var scope = _serviceProvider.CreateScope();

                var handler = scope.ServiceProvider.GetRequiredService(handlerType)
                              ?? throw new InvalidOperationException(
                                  $"Handler {handlerType.Name} not found in DI");

                var method = handlerType.GetMethod("Handle")
                             ?? throw new InvalidOperationException(
                                 $"Handler {handlerType.Name} has no Handle() method");

                var task = method.Invoke(handler, new object[] { integrationEvent })
                           ?? throw new InvalidOperationException(
                               $"Handler {handlerType.Name} returned null Task");

                await (Task)task;
            }
        }


        public void Dispose()
        {
            _cts.Cancel();
        }
    }

}
