using System;
using System.Collections.Generic;

using Shared.RabbitMQ.rpc.Abstraction;

namespace Shared.RabbitMQ.rpc.Contracts.GetPostCommentsCounts
{
    public class GetPostCommentsCountsRpcResponse : IRPCResponse
    {
        public Dictionary<Guid, int> Counts { get; set; } = new();
    }
}
