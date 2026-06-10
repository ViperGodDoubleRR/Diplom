using AuthService.Domain.Interface;
using MediatR;

using Microsoft.Extensions.Configuration;

using Shared.Application.Contracts;
using Shared.Application.Contracts.AuthJWT;
using Shared.Application.Interfaces;
namespace AuthService.Application.MediatR.RefreshToken
{
    public class RefreshTokenHandler
        : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthGoResponse>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IHasher _hasher;
        private readonly IConfiguration _configuration;

        public RefreshTokenHandler(
            IAuthRepository authRepository,
            IJwtProvider jwtProvider,
            IHasher hasher,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _jwtProvider = jwtProvider;
            _hasher = hasher;
            _configuration = configuration;
        }

        public async Task<ApiResponse<AuthGoResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Fail("INVALID_REFRESH_TOKEN", "Refresh token обязателен");
            }

            var sessions = await _authRepository.GetActiveSessions(cancellationToken);
            var session = sessions.FirstOrDefault(x =>
                _hasher.Verify(request.RefreshToken, x.RefreshToken));

            if (session == null)
            {
                return Fail("INVALID_REFRESH_TOKEN", "Refresh token недействителен или просрочен");
            }

            var newAccessToken = _jwtProvider.GenerateAccessToken(
                session.User.Id,
                session.User.Email,
                session.User.Login);

            var newRefreshToken = _jwtProvider.GenerateRefreshToken();
            var newRefreshTokenHash = _hasher.Hash(newRefreshToken);

            var refreshLifetimeDays = int.Parse(
                _configuration["Jwt:RefreshTokenLifetimeDays"] ?? "30");

            var expiresAt = DateTime.UtcNow.AddDays(refreshLifetimeDays);

            await _authRepository.UpdateSessionRefreshTokenAsync(
                session.Id,
                newRefreshTokenHash,
                null,
                expiresAt,
                cancellationToken);

            return new ApiResponse<AuthGoResponse>
            {
                Success = true,
                Data = new AuthGoResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                }
            };
        }

        private static ApiResponse<AuthGoResponse> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
