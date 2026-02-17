using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.CreateUser
{
    public class CreateUserRpcCall:IRPCEvent
    {
        public string ResponseType => typeof(CreateUserRpcResponse).AssemblyQualifiedName;
        public string Email=string.Empty;
        public string Password=string.Empty;
        public string Login=string.Empty;
    }
}
