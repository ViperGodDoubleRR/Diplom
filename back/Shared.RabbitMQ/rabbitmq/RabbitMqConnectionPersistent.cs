using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
namespace Shared.RabbitMQ.RabbitMq
{
    public class RabbitMqConnectionPersistent
    {
        private readonly RabbitMqConnection _connection;

        public RabbitMqConnectionPersistent(RabbitMqOptions options)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password
            };

            _connection = new RabbitMqConnection(factory);
        }

        public Task<IChannel> CreateChannelAsync()
            => _connection.CreateChannelAsync();
    }
}
