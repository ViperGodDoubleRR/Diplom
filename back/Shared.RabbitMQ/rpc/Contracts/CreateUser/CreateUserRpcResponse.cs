using System;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.CreateUser
{
    public class CreateUserRpcResponse:IRPCResponse
    {
        public Guid id { get; set; }
        public string Email { get; set; }=string.Empty;
        public string Login { get; set; }=string.Empty;

    }
}
