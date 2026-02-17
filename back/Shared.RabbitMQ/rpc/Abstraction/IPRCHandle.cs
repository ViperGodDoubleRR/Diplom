using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RabbitMQ.rpc.Abstraction
{

    public interface IRPCHandle<TRequest, TResponse>
     where TRequest : IRPCEvent
     where TResponse : IRPCResponse
    {
        Task<TResponse> Handle(TRequest request);
    }


}
