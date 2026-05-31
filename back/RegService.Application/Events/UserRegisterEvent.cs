using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;

namespace RegService.Application.Events
{
    public class UserRegisterHandlerEvent : IEventHandler<CreateUserEvent>
    {
        public Task Handle(CreateUserEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
