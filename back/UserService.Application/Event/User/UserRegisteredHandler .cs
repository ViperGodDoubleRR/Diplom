using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;
using UserService.Domain.IRepository;
using UserService.Domain.Models;
namespace UserService.Application.Event.UserCreate
{
    public class UserRegisteredHandler : IEventHandler<CreateUserEvent>
    {
        private readonly IUserRepository _repo;

        public UserRegisteredHandler(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(CreateUserEvent @event)
        {
            var user = new User
            {
                Id = @event.id,
                Email = @event.Email,
                Login = @event.Login,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.CreateUser(user, default);

            Console.WriteLine($"✅ USER CREATED: {@event.Email}");
        }
    }
}
