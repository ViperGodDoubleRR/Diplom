using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.UpdateUserEmail
{
    public class UpdateUserEmailRpcResponse : IRPCResponse
    {
        public bool Success { get; set; } = true;
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
