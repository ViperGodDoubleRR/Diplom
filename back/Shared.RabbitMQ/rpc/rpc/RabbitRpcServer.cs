using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared.RabbitMQ.RabbitMq;
using Shared.RabbitMQ.rpc.Abstraction;

public class RabbitRpcServer : IRpcServer
{
    private readonly RabbitMqConnectionPersistent _connection;
    private readonly IServiceProvider _provider;

    public RabbitRpcServer(RabbitMqConnectionPersistent connection, IServiceProvider provider)
    {
        _connection = connection;
        _provider = provider;
    }

    public async void Start(string rpcQueue)
    {
        var channel = await _connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: rpcQueue,
            durable: false,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var typeName = ea.BasicProperties.Type
                               ?? throw new InvalidOperationException("RPC request missing Type header");

                var requestType = Type.GetType(typeName)
                                  ?? throw new InvalidOperationException($"Cannot resolve request type: {typeName}");

                var request = JsonSerializer.Deserialize(json, requestType)
                              ?? throw new InvalidOperationException("Failed to deserialize RPC request");

                var rpcEvent = request as IRPCEvent
                               ?? throw new InvalidOperationException("Request does not implement IRPCEvent");

                var responseTypeName = rpcEvent.ResponseType
                                       ?? throw new InvalidOperationException("ResponseType is null");

                var responseType = Type.GetType(responseTypeName)
                                   ?? throw new InvalidOperationException($"Cannot resolve response type: {responseTypeName}");

                var handlerType = typeof(IRPCHandle<,>).MakeGenericType(requestType, responseType);

                using var scope = _provider.CreateScope();
                var scopedProvider = scope.ServiceProvider;

                var handler = scopedProvider.GetService(handlerType)
                    ?? throw new InvalidOperationException($"Handler not registered: {handlerType}");

                var handleMethod = handlerType.GetMethod("Handle")
                    ?? throw new InvalidOperationException("Handler has no Handle() method");

                var task = (Task)handleMethod.Invoke(handler, new[] { request });
                await task;

                var resultProperty = task.GetType().GetProperty("Result");
                var responseObj = resultProperty.GetValue(task);


                var responseJson = JsonSerializer.Serialize(responseObj);
                var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                var correlationId = ea.BasicProperties.CorrelationId
                                    ?? throw new InvalidOperationException("CorrelationId is missing");

                var replyTo = ea.BasicProperties.ReplyTo
                              ?? throw new InvalidOperationException("ReplyTo is missing");

                var props = new BasicProperties();
                props.CorrelationId = correlationId;

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: replyTo,
                    mandatory: false,
                    basicProperties: props,
                    body: responseBytes
                );

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RPC SERVER ERROR: " + ex);
            }
        };

        await channel.BasicConsumeAsync(
            queue: rpcQueue,
            autoAck: false,
            consumerTag: "",
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer
        );
    }
}

