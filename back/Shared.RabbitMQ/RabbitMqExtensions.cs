using Microsoft.Extensions.DependencyInjection;

using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.RabbitMQ;
using Shared.RabbitMQ.EventBus.Subscriptions;
using Shared.RabbitMQ.RabbitMq;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.rpcRabbitMQ;
namespace Shared.RabbitMQ
{

    public static class RabbitMqExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton(new RabbitMqOptions
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            });

            services.AddSingleton<RabbitMqConnectionPersistent>();

            services.AddSingleton<SubscriptionManager>();
            services.AddSingleton<IEventBus, RabbitMqEventBus>();

            services.AddSingleton<IRpcClient, RabbitRpcClient>();
            services.AddSingleton<IRpcServer, RabbitRpcServer>();

            services.Scan(scan => scan
                .FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            services.Scan(scan => scan
                .FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo(typeof(IRPCHandle<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
            return services;
        }
    }
}
