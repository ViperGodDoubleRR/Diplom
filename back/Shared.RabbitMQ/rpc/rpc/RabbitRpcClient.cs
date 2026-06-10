using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared.RabbitMQ.RabbitMq;
using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.rpcRabbitMQ
{
    public class RabbitRpcClient : IRpcClient
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        private readonly RabbitMqConnectionPersistent _connection;

        public RabbitRpcClient(RabbitMqConnectionPersistent connection)
        {
            _connection = connection;
        }

        public async Task<TResponse> CallAsync<TRequest, TResponse>(string rpcQueue, TRequest request)
            where TRequest : IRPCEvent
            where TResponse : IRPCResponse
        {
            IChannel? channel = null;

            try
            {
                channel = await _connection.CreateChannelAsync();

                var reply = await channel.QueueDeclareAsync(
                    queue: "",
                    durable: false,
                    exclusive: true,
                    autoDelete: true
                );

                var replyQueue = reply.QueueName
                                 ?? throw new InvalidOperationException("Reply queue name is null");

                var correlationId = Guid.NewGuid().ToString();

                var props = new BasicProperties();
                props.CorrelationId = correlationId;
                props.ReplyTo = replyQueue;
                props.Type = typeof(TRequest).AssemblyQualifiedName
                             ?? throw new InvalidOperationException("Cannot resolve request type name");

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: rpcQueue,
                    mandatory: false,
                    basicProperties: props,
                    body: body
                );

                var tcs = new TaskCompletionSource<TResponse>();

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var eaCorrelation = ea.BasicProperties.CorrelationId;

                    if (eaCorrelation != null && eaCorrelation == correlationId)
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var response = JsonSerializer.Deserialize<TResponse>(json)
                                       ?? throw new InvalidOperationException("Failed to deserialize RPC response");

                        tcs.TrySetResult(response);
                    }

                    await Task.Yield();
                };

                await channel.BasicConsumeAsync(
                    queue: replyQueue,
                    autoAck: true,
                    consumerTag: "",
                    noLocal: false,
                    exclusive: false,
                    arguments: null,
                    consumer: consumer
                );

                return await tcs.Task.WaitAsync(DefaultTimeout);
            }
            finally
            {
                if (channel is not null)
                {
                    try
                    {
                        await channel.CloseAsync();
                    }
                    catch
                    {
                        /* ignore */
                    }

                    channel.Dispose();
                }
            }
        }
    }
}
