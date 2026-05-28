using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.GetUserPost
{
    public class GetUserRpcResponse : IRPCResponse
    {
        public Guid Id { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
