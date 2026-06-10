using System;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.EmailExists
{
    public class EmailExistsRpcRequest : IRPCEvent
    {
        public string Email { get; set; } = string.Empty;
        public Guid? ExcludeUserId { get; set; }

        public string ResponseType =>
            typeof(EmailExistsRpcResponse).AssemblyQualifiedName!;
    }
}
