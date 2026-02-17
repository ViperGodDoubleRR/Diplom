using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RabbitMQ.EventBus.Abstractions
{
    public interface IEventHandler<TEvent> where TEvent:IIntegrationEvent 
    {
        Task Handle(TEvent @event);
    }
}
