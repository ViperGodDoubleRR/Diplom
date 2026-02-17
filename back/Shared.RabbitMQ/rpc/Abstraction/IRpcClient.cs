using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RabbitMQ.rpc.Abstraction
{
    public interface IRpcClient
    {
        Task<TResponse> CallAsync<TRequest, TResponse>(string rpcQueue, TRequest request)
            where TRequest : IRPCEvent
            where TResponse : IRPCResponse;
    }

}
