using System;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.UpdateUserEmail
{
    public class UpdateUserEmailRpcRequest : IRPCEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;

        public string ResponseType =>
            typeof(UpdateUserEmailRpcResponse).AssemblyQualifiedName!;
    }
}
