using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.EmailExists
{
    public class EmailExistsRpcResponse : IRPCResponse
    {
        public bool Exists { get; set; }
    }
}
