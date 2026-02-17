using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
namespace Shared.RabbitMQ.RabbitMq
{
    public  class RabbitMqConnection
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _iconnection;

        public RabbitMqConnection(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task ConnectAsync()
        {
            _iconnection = await _connectionFactory.CreateConnectionAsync();
            
        }
        public async Task<IChannel> CreateNewChannel()
        {
            var channel = await _iconnection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync("social", "topic");
            return channel;
        }

    }
}
