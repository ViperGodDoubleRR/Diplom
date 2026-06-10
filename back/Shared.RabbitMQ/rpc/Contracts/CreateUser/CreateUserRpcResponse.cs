using System;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.CreateUser
{
    public class CreateUserRpcResponse:IRPCResponse
    {
        public bool Success { get; set; } = true;
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid Id { get; set; }
        public string Email { get; set; }=string.Empty;
        public string Login { get; set; }=string.Empty;
    }
}
