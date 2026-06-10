using AuthService.Domain.Interface;

using Microsoft.Extensions.Logging;

using Shared.Application.Validation;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.EmailExists;

namespace AuthService.Application.Settings
{
    public class EmailChangeAvailabilityChecker
    {
        private readonly IAuthRepository _authRepository;
        private readonly IRpcClient _rpcClient;
        private readonly ILogger<EmailChangeAvailabilityChecker> _logger;

        public EmailChangeAvailabilityChecker(
            IAuthRepository authRepository,
            IRpcClient rpcClient,
            ILogger<EmailChangeAvailabilityChecker> logger)
        {
            _authRepository = authRepository;
            _rpcClient = rpcClient;
            _logger = logger;
        }

        public async Task<(bool Available, string? ErrorCode, string? ErrorMessage)> CheckAsync(
            Guid userId,
            string newEmail,
            CancellationToken cancellationToken)
        {
            if (!InputValidator.IsValidEmail(newEmail))
                return (false, "INVALID_EMAIL", "Некорректный email");

            var normalizedEmail = InputValidator.NormalizeEmail(newEmail);
            var user = await _authRepository.GetUserByIdAsync(userId, cancellationToken);

            if (user is null)
                return (false, "USER_NOT_FOUND", "Пользователь не найден");

            if (string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                return (false, "SAME_EMAIL", "Это уже ваш текущий email");

            if (await _authRepository.EmailExists(normalizedEmail, cancellationToken))
                return (false, "EMAIL_EXISTS", "Email уже используется");

            if (await IsEmailTakenInServiceAsync("user.rpc", normalizedEmail, userId))
                return (false, "EMAIL_EXISTS", "Email уже используется");

            if (await IsEmailTakenInServiceAsync("reg.rpc", normalizedEmail, userId))
                return (false, "EMAIL_EXISTS", "Email уже используется");

            return (true, null, null);
        }

        private async Task<bool> IsEmailTakenInServiceAsync(
            string queue,
            string email,
            Guid excludeUserId)
        {
            try
            {
                var response = await _rpcClient.CallAsync<
                    EmailExistsRpcRequest,
                    EmailExistsRpcResponse>(
                    queue,
                    new EmailExistsRpcRequest
                    {
                        Email = email,
                        ExcludeUserId = excludeUserId
                    });

                return response.Exists;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Email exists RPC failed for queue {Queue}", queue);
                return false;
            }
        }
    }
}
