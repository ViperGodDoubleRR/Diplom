using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
namespace Shared.RabbitMQ.RabbitMq
{
    public class RabbitMqConnection
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;

        public RabbitMqConnection(ConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            if (_connection != null)
                return _connection;

            _connection = await _factory.CreateConnectionAsync();
            return _connection;
        }

        public async Task<IChannel> CreateChannelAsync()
        {
            var conn = await GetConnectionAsync();
            return await conn.CreateChannelAsync();
        }
    }
}
