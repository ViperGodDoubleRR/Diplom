using System;
using System.Collections.Generic;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.GetPostCommentsCounts
{
    public class GetPostCommentsCountsRpcRequest : IRPCEvent
    {
        public List<Guid> PostIds { get; set; } = [];

        public string ResponseType =>
            typeof(GetPostCommentsCountsRpcResponse).AssemblyQualifiedName!;
    }
}
