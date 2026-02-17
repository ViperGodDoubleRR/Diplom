using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events;
using MediatR;

namespace RegService.Application.Events
{
    public class UserRegisterHandlerEvent : IEventHandler<UserRegisterEvent>
    {
        private readonly IMediator _mediator;
        public UserRegisterHandlerEvent(IMediator mediator)
        {
            _mediator=mediator;
        }

        public async Task Handle(UserRegisterEvent @event)
        {
            await _mediator.Send(@event);

        }
    }
}
