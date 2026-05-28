using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.RabbitMQ.EventBus.Abstractions;
using MediatR;
using Shared.RabbitMQ.EventBus.Events.User;

namespace RegService.Application.Events
{
    public class UserRegisterHandlerEvent : IEventHandler<CreateUserEvent>
    {
        private readonly IMediator _mediator;
        public UserRegisterHandlerEvent(IMediator mediator)
        {
            _mediator=mediator;
        }

        public async Task Handle(CreateUserEvent @event)
        {
            await _mediator.Send(@event);

        }
    }
}
