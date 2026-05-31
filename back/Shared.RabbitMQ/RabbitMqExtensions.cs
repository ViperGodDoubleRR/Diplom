using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using RabbitMQ.Client;

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
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,
            IConfiguration? configuration = null)
        {
            services.AddSingleton(new RabbitMqOptions
            {
                HostName = configuration?["RabbitMQ:Host"] ?? "localhost",
                UserName = configuration?["RabbitMQ:UserName"] ?? "guest",
                Password = configuration?["RabbitMQ:Password"] ?? "guest",
                Port = int.TryParse(configuration?["RabbitMQ:Port"], out var port) ? port : 5672
            });

            services.AddSingleton<RabbitMqConnection>(sp =>
            {
                var opt = sp.GetRequiredService<RabbitMqOptions>();

                return new RabbitMqConnection(
                    new ConnectionFactory
                    {
                        HostName = opt.HostName,
                        UserName = opt.UserName,
                        Password = opt.Password,
                        Port = opt.Port
                    });
            });

            services.AddSingleton<RabbitMqConnectionPersistent>(sp =>
            {
                var opt = sp.GetRequiredService<RabbitMqOptions>();
                return new RabbitMqConnectionPersistent(opt);
            });

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
    .WithScopedLifetime());

            return services;
        }
    }
}
