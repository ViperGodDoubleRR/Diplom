using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.RabbitMQ.EventBus.Abstractions;

namespace Shared.RabbitMQ.EventBus.Events
{
    public class UserRegisterEvent:IIntegrationEvent
    {
        public Guid id { get; set;  }
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
