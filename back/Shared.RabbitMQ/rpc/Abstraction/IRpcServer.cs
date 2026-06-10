using System.Threading;
using System.Threading.Tasks;

namespace Shared.RabbitMQ.rpc.Abstraction
{
    public interface IRpcServer
    {
        Task StartAsync(string rpcQueue, CancellationToken cancellationToken = default);
    }
}
