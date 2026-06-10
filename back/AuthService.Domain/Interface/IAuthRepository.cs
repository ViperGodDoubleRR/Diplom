using AuthService.Domain.Models;

namespace AuthService.Domain.Interface
{
    public interface IAuthRepository
    {
        Task<Guid?> CreateUser(string email, string login, string passwordHash);
        Task<bool> EmailExists(string email, CancellationToken cancellationToken = default);
        Task<bool> LoginExists(string login, CancellationToken cancellationToken = default);
        Task<bool> RequestCode(string email, string code, CancellationToken cancellationToken);
        Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken);
        Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
        Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CreateVerificationCodeForUserAsync(
            Guid userId,
            string code,
            CancellationToken cancellationToken = default);
        Task<bool> VerifyUserCodeAsync(Guid userId, string code, CancellationToken cancellationToken = default);
        Task<bool> IsEmailUsedByUserHistoryAsync(
            Guid userId,
            string email,
            CancellationToken cancellationToken = default);
        Task<bool> UpdateUserEmailAsync(Guid userId, string newEmail, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserPasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default);
        Task<List<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserSession?> FindActiveSessionByFingerprintAsync(
            string fingerprint,
            CancellationToken cancellationToken = default);
        Task<bool> RevokeSessionAsync(int sessionId, Guid userId, CancellationToken cancellationToken = default);
        Task<int> RevokeOtherSessionsAsync(
            Guid userId,
            int? exceptSessionId,
            CancellationToken cancellationToken = default);
        Task<bool> CanRequestVerificationCodeAsync(
            Guid userId,
            TimeSpan cooldown,
            CancellationToken cancellationToken = default);
        Task CreateSession(
            Guid userId,
            string refreshTokenHash,
            string? tokenFingerprint,
            string deviceInfo,
            string ipAddress,
            DateTime expiresAt,
            CancellationToken cancellationToken);
        Task UpdateSessionRefreshTokenAsync(
            int sessionId,
            string newRefreshTokenHash,
            string? tokenFingerprint,
            DateTime expiresAt,
            CancellationToken cancellationToken);
        Task<List<UserSession>> GetActiveSessions(CancellationToken cancellationToken);
        Task RevokeAllUserSessions(Guid userId, CancellationToken cancellationToken = default);
        Task InvalidateVerificationCodes(string email, CancellationToken cancellationToken = default);
    }
}
