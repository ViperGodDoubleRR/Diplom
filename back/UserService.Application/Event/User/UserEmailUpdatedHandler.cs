using Microsoft.Extensions.Logging;

using Shared.Application.Validation;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;

using UserService.Domain.IRepository;

namespace UserService.Application.Event.UserEmail
{
    public class UserEmailUpdatedHandler : IEventHandler<UpdateUserEmailEvent>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserEmailUpdatedHandler> _logger;

        public UserEmailUpdatedHandler(
            IUserRepository userRepository,
            ILogger<UserEmailUpdatedHandler> logger)
        {
            _userRepository = userRepository;
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
            var updated = await _userRepository.UpdateEmailAsync(@event.UserId, email);

            if (!updated)
            {
                _logger.LogWarning(
                    "User profile email not updated for {UserId}",
                    @event.UserId);
                return;
            }

            _logger.LogInformation(
                "User profile email updated: {UserId} ({Email})",
                @event.UserId,
                email);
        }
    }
}
