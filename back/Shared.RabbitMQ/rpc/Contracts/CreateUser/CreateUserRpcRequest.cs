using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.CreateUser
{
    public class CreateUserRpcRequest:IRPCEvent
    {
        public string ResponseType => typeof(CreateUserRpcResponse).AssemblyQualifiedName;
        public string Email { get; set; } =string.Empty;
        public string Password { get; set; } =string.Empty;
        public string Login { get; set; } =string.Empty;
    }
}
