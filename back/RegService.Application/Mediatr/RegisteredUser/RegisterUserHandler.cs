using MediatR;

using Microsoft.Extensions.Logging;

using RegService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.Application.Validation;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.CreateUser;

namespace RegService.Application.Mediatr.RegisteredUser
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ApiResponse<string>>
    {
        private readonly IRegRepository _regRepository;
        private readonly IRpcClient _rpcClient;
        private readonly IEventBus _eventBus;
        private readonly ILogger<RegisterUserHandler> _logger;

        public RegisterUserHandler(
            IRegRepository repository,
            IRpcClient rpcClient,
            IEventBus eventBus,
            ILogger<RegisterUserHandler> logger)
        {
            _regRepository = repository;
            _rpcClient = rpcClient;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(
            RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            if (!InputValidator.IsValidEmail(command.Email))
            {
                return Fail("INVALID_EMAIL", "Некорректный email");
            }

            if (!InputValidator.IsValidLogin(command.Login, out var loginError))
            {
                return Fail("INVALID_LOGIN", loginError!);
            }

            if (!InputValidator.IsValidPassword(command.Password, out var passwordError))
            {
                return Fail("INVALID_PASSWORD", passwordError!);
            }

            var email = InputValidator.NormalizeEmail(command.Email);
            var login = command.Login.Trim();

            if (!await _regRepository.IsEmailConfirmed(email, cancellationToken))
            {
                return Fail("EMAIL_NOT_CONFIRMED", "Сначала подтвердите email кодом");
            }

            CreateUserRpcResponse response;

            try
            {
                response = await _rpcClient.CallAsync<CreateUserRpcRequest, CreateUserRpcResponse>(
                    "auth.rpc",
                    new CreateUserRpcRequest
                    {
                        Email = email,
                        Password = command.Password,
                        Login = login
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth RPC failed for {Email}", email);
                return Fail("AUTH_SERVICE_UNAVAILABLE", "Сервис авторизации недоступен");
            }

            if (!response.Success)
            {
                return Fail(
                    response.ErrorCode ?? "CREATE_USER_FAILED",
                    response.ErrorMessage ?? "Не удалось создать пользователя");
            }

            var registered = await _regRepository.RegisterUser(
                response.Id,
                response.Email,
                response.Login,
                cancellationToken);

            if (!registered)
            {
                _logger.LogWarning(
                    "RegDb save failed after Auth user created: {UserId} {Email}",
                    response.Id,
                    response.Email);

                return Fail("REGISTRATION_FAILED", "Не удалось завершить регистрацию");
            }

            await _eventBus.Publish(new CreateUserEvent
            {
                id = response.Id,
                Email = response.Email,
                Login = response.Login
            });

            return new ApiResponse<string>
            {
                Success = true,
                Data = "REGISTRATION_COMPLETE"
            };
        }

        private static ApiResponse<string> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
