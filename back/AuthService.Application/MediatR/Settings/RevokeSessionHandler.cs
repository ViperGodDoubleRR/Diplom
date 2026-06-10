using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;

namespace AuthService.Application.MediatR.Settings
{
    public class RevokeSessionHandler
        : IRequestHandler<RevokeSessionCommand, ApiResponse<RevokeSessionResult>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHasher _hasher;

        public RevokeSessionHandler(IAuthRepository authRepository, IHasher hasher)
        {
            _authRepository = authRepository;
            _hasher = hasher;
        }

        public async Task<ApiResponse<RevokeSessionResult>> Handle(
            RevokeSessionCommand request,
            CancellationToken cancellationToken)
        {
            var sessions = await _authRepository.GetActiveSessionsByUserIdAsync(
                request.UserId,
                cancellationToken);

            var session = sessions.FirstOrDefault(x => x.Id == request.SessionId);

            if (session is null)
            {
                return new ApiResponse<RevokeSessionResult>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "SESSION_NOT_FOUND",
                        Message = "Сессия не найдена"
                    }
                };
            }

            var currentSessionId = await CurrentSessionResolver.ResolveAsync(
                _authRepository,
                _hasher,
                request.UserId,
                request.RefreshToken,
                cancellationToken);

            var wasCurrent = currentSessionId.HasValue && currentSessionId.Value == request.SessionId;

            var revoked = await _authRepository.RevokeSessionAsync(
                request.SessionId,
                request.UserId,
                cancellationToken);

            if (!revoked)
            {
                return new ApiResponse<RevokeSessionResult>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "SESSION_NOT_FOUND",
                        Message = "Сессия не найдена"
                    }
                };
            }

            return new ApiResponse<RevokeSessionResult>
            {
                Success = true,
                Data = new RevokeSessionResult
                {
                    Revoked = true,
                    WasCurrentSession = wasCurrent
                }
            };
        }
    }
}
