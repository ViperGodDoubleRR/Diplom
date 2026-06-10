using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;

using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.Event.UserCreate
{
    public class UserRegisteredHandler : IEventHandler<CreateUserEvent>
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<UserRegisteredHandler> _logger;

        public UserRegisteredHandler(
            IUserRepository repo,
            ILogger<UserRegisteredHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task Handle(CreateUserEvent @event)
        {
            if (@event.id == Guid.Empty)
            {
                _logger.LogWarning("CreateUserEvent received with empty user id");
                return;
            }

            if (await _repo.ExistsByIdAsync(@event.id))
            {
                _logger.LogInformation(
                    "User {UserId} already exists, skipping duplicate event",
                    @event.id);
                return;
            }

            var user = new User
            {
                Id = @event.id,
                Email = @event.Email.Trim().ToLowerInvariant(),
                Login = @event.Login.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.CreateUser(user);

            _logger.LogInformation("User profile created: {UserId} ({Login})", user.Id, user.Login);
        }
    }
}
