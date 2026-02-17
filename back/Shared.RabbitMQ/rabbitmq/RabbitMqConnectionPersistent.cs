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
        private readonly ConnectionFactory _connectionFactory;
        private readonly RabbitMqConnection _connection;
        public RabbitMqConnectionPersistent(RabbitMqOptions options)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,

                HandshakeContinuationTimeout = TimeSpan.FromSeconds(60),
                RequestedConnectionTimeout= TimeSpan.FromSeconds(60)
            };
            _connection = new RabbitMqConnection(_connectionFactory);
            
            _connection.ConnectAsync().Wait();
        }
        public async Task<IChannel> CreateChannelAsync()
        {
            return await _connection.CreateNewChannel();
        }
    }
}
