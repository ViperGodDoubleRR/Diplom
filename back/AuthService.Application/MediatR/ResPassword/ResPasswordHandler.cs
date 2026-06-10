using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.Application.Validation;

namespace AuthService.Application.MediatR.ResPassword
{
    public class ResPasswordHandler : IRequestHandler<ResPasswordCommand, ApiResponse<string>>
    {
        private readonly IResRepository _resRepository;
        private readonly IHasher _hasher;
        private readonly IJwtProvider _jwtProvider;

        public ResPasswordHandler(
            IResRepository resRepository,
            IHasher hasher,
            IJwtProvider jwtProvider)
        {
            _resRepository = resRepository;
            _hasher = hasher;
            _jwtProvider = jwtProvider;
        }

        public async Task<ApiResponse<string>> Handle(
            ResPasswordCommand command,
            CancellationToken cancellationToken)
        {
            if (!InputValidator.IsValidEmail(command.Email))
            {
                return Fail("INVALID_EMAIL", "Некорректный email");
            }

            if (!InputValidator.IsValidPassword(command.NewPassword, out var passwordError))
            {
                return Fail("INVALID_PASSWORD", passwordError!);
            }

            if (string.IsNullOrWhiteSpace(command.ResetToken))
            {
                return Fail("RESET_TOKEN_REQUIRED", "Сначала подтвердите код сброса пароля");
            }

            var tokenData = _jwtProvider.ValidatePasswordResetToken(command.ResetToken);
            var email = InputValidator.NormalizeEmail(command.Email);

            if (tokenData == null || tokenData.Email != email)
            {
                return Fail("INVALID_RESET_TOKEN", "Токен сброса недействителен или просрочен");
            }

            var passwordHash = _hasher.Hash(command.NewPassword);
            var success = await _resRepository.ResetPassword(email, passwordHash, cancellationToken);

            if (!success)
            {
                return Fail("RESET_PASSWORD_FAILED", "Не удалось сменить пароль");
            }

            return new ApiResponse<string>
            {
                Success = true,
                Data = "PASSWORD_CHANGED"
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
