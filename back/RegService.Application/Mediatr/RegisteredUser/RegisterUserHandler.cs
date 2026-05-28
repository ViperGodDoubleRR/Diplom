
using MediatR;
using RegService.Domain.IRepository;
using RegService.Domain.Models;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.CreateUser;

namespace RegService.Application.Mediatr.RegisteredUser
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ApiResponse<string>>
    {
        private readonly IRegRepository _regRepository;
        private readonly IHasher _hasher;
        private readonly IRpcClient _rpcclient;
        private readonly IEventBus _eventBus;
        public RegisterUserHandler(IRegRepository repository, IHasher hasher, IRpcClient rpcclient, IEventBus eventBus)
        {
            _regRepository = repository;
            _hasher = hasher;
            _rpcclient=rpcclient;
            _eventBus = eventBus;
        }
        public async Task<ApiResponse<string>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var call = new CreateUserRpcRequest
            {
                Email = command.Email,
                Password = command.Password,
                Login = command.Login
            };

            var response = await _rpcclient.CallAsync<CreateUserRpcRequest, CreateUserRpcResponse>(
                "auth.rpc",
                call);

            await _regRepository.RegisterUser(
                response.Id,
                response.Email,
                response.Login,
                cancellationToken);

            // 🔥 ПУБЛИКАЦИЯ EVENT
            await _eventBus.Publish(new CreateUserEvent
            {
                id = response.Id,
                Email = response.Email,
                Login = response.Login
            });

            return new ApiResponse<string>
            {
                Success = true,
                Data = "Пользователь зарегистрирован"
            };
        }
    }
}
