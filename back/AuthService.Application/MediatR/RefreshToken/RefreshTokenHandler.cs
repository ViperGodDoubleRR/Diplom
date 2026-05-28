using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Domain.Interface;

using MediatR;

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
        public RefreshTokenHandler(IAuthRepository authRepository,IJwtProvider jwtProvider,IHasher hasher)
        {
            _authRepository = authRepository;
            _jwtProvider = jwtProvider;
            _hasher = hasher;
        }

        public async Task<ApiResponse<AuthGoResponse>> Handle(RefreshTokenCommand request,CancellationToken cancellationToken)
        {
            var sessions = await _authRepository
                .GetActiveSessions(cancellationToken);

            var session = sessions.FirstOrDefault(x =>
                _hasher.Verify(request.RefreshToken, x.RefreshToken));

            if (session == null)
            {
                return new ApiResponse<AuthGoResponse>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "INVALID_REFRESH_TOKEN",
                        Message = "Refresh token is invalid or expired"
                    }
                };
            }

            var newAccessToken = _jwtProvider.GenerateAccessToken(
                session.User.Id,
                session.User.Email,
                session.User.Login
            );

            var newRefreshToken = _jwtProvider.GenerateRefreshToken();

            var newRefreshTokenHash = _hasher.Hash(newRefreshToken);

            await _authRepository.UpdateSessionRefreshTokenAsync(
                session.Id,
                newRefreshTokenHash,
                DateTime.UtcNow.AddDays(30),
                cancellationToken
            );

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
    }
}
