using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.CreateUser;

namespace Shared.RabbitMQ.rpc.Contracts.GetUserPost
{
    public class GetUserRpcRequest : IRPCEvent
    {
        public Guid UserId { get; set; }

        public string ResponseType =>
            typeof(GetUserRpcResponse).AssemblyQualifiedName!;
    }
}
