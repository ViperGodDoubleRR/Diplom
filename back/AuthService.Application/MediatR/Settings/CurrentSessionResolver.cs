using AuthService.Domain.Interface;

using Shared.Application.Interfaces;

namespace AuthService.Application.MediatR.Settings
{
    internal static class CurrentSessionResolver
    {
        public static async Task<int?> ResolveAsync(
            IAuthRepository repository,
            IHasher hasher,
            Guid userId,
            string? refreshToken,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var sessions = await repository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);

            return sessions
                .FirstOrDefault(x => hasher.Verify(refreshToken, x.RefreshToken))
                ?.Id;
        }
    }
}
