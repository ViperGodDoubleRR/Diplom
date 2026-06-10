using AuthService.Domain.Interface;

using Shared.Application.Interfaces;
using Shared.Application.Validation;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.CreateUser;

namespace AuthService.Application.Events
{
    public class CreateUser : IRPCHandle<CreateUserRpcRequest, CreateUserRpcResponse>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHasher _hasher;

        public CreateUser(IAuthRepository authRepository, IHasher hasher)
        {
            _hasher = hasher;
            _authRepository = authRepository;
        }

        public async Task<CreateUserRpcResponse> Handle(CreateUserRpcRequest request)
        {
            if (!InputValidator.IsValidEmail(request.Email))
            {
                return Fail("INVALID_EMAIL", "Некорректный email");
            }

            if (!InputValidator.IsValidLogin(request.Login, out var loginError))
            {
                return Fail("INVALID_LOGIN", loginError!);
            }

            if (!InputValidator.IsValidPassword(request.Password, out var passwordError))
            {
                return Fail("INVALID_PASSWORD", passwordError!);
            }

            var email = InputValidator.NormalizeEmail(request.Email);
            var login = request.Login.Trim();

            if (await _authRepository.EmailExists(email))
            {
                return Fail("EMAIL_EXISTS", "Email уже зарегистрирован");
            }

            if (await _authRepository.LoginExists(login))
            {
                return Fail("LOGIN_EXISTS", "Логин уже занят");
            }

            var passwordHash = _hasher.Hash(request.Password);
            var id = await _authRepository.CreateUser(email, login, passwordHash);

            if (id == null)
            {
                return Fail("CREATE_USER_FAILED", "Не удалось создать пользователя");
            }

            return new CreateUserRpcResponse
            {
                Success = true,
                Id = id.Value,
                Email = email,
                Login = login
            };
        }

        private static CreateUserRpcResponse Fail(string code, string message) =>
            new()
            {
                Success = false,
                ErrorCode = code,
                ErrorMessage = message
            };
    }
}
