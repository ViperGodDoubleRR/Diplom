using Microsoft.Extensions.Logging;

using RegService.Domain.IRepository;

using Shared.Application.Validation;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;

namespace RegService.Application.Event.User
{
    public class UserEmailUpdatedHandler : IEventHandler<UpdateUserEmailEvent>
    {
        private readonly IRegRepository _regRepository;
        private readonly ILogger<UserEmailUpdatedHandler> _logger;

        public UserEmailUpdatedHandler(
            IRegRepository regRepository,
            ILogger<UserEmailUpdatedHandler> logger)
        {
            _regRepository = regRepository;
            _logger = logger;
        }

        public async Task Handle(UpdateUserEmailEvent @event)
        {
            if (@event.UserId == Guid.Empty)
            {
                _logger.LogWarning("UpdateUserEmailEvent received with empty user id");
                return;
            }

            var email = InputValidator.NormalizeEmail(@event.Email);
            var updated = await _regRepository.UpdateEmailAsync(@event.UserId, email);

            if (!updated)
            {
                _logger.LogWarning(
                    "Reg profile email not updated for {UserId}",
                    @event.UserId);
                return;
            }

            _logger.LogInformation(
                "Reg profile email updated: {UserId} ({Email})",
                @event.UserId,
                email);
        }
    }
}
