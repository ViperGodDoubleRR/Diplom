using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.Application.Validation;

namespace AuthService.Application.MediatR.Settings
{
    public class ChangePasswordHandler
        : IRequestHandler<ChangePasswordCommand, ApiResponse<string>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHasher _hasher;

        public ChangePasswordHandler(IAuthRepository authRepository, IHasher hasher)
        {
            _authRepository = authRepository;
            _hasher = hasher;
        }

        public async Task<ApiResponse<string>> Handle(
            ChangePasswordCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.CurrentPassword))
                return Fail("CURRENT_PASSWORD_REQUIRED", "Введите текущий пароль");

            if (!InputValidator.IsValidPassword(command.NewPassword, out var passwordError))
                return Fail("INVALID_PASSWORD", passwordError!);

            var user = await _authRepository.GetUserByIdAsync(command.UserId, cancellationToken);

            if (user is null)
                return Fail("USER_NOT_FOUND", "Пользователь не найден");

            if (!_hasher.Verify(command.CurrentPassword, user.PasswordHash))
                return Fail("INVALID_PASSWORD", "Неверный текущий пароль");

            if (_hasher.Verify(command.NewPassword, user.PasswordHash))
                return Fail("SAME_PASSWORD", "Новый пароль должен отличаться от текущего");

            var passwordHash = _hasher.Hash(command.NewPassword);
            var updated = await _authRepository.UpdateUserPasswordAsync(
                command.UserId,
                passwordHash,
                cancellationToken);

            if (!updated)
                return Fail("UPDATE_FAILED", "Не удалось сменить пароль");

            var currentSessionId = await CurrentSessionResolver.ResolveAsync(
                _authRepository,
                _hasher,
                command.UserId,
                command.RefreshToken,
                cancellationToken);

            await _authRepository.RevokeOtherSessionsAsync(
                command.UserId,
                currentSessionId,
                cancellationToken);

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
