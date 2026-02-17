using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RabbitMQ.rpc.Abstraction
{
    public interface IRPCEvent
    {
        string ResponseType { get; }
    }
}
