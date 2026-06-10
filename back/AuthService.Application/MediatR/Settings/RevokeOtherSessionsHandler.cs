using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;

namespace AuthService.Application.MediatR.Settings
{
    public class RevokeOtherSessionsHandler
        : IRequestHandler<RevokeOtherSessionsCommand, ApiResponse<int>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHasher _hasher;

        public RevokeOtherSessionsHandler(IAuthRepository authRepository, IHasher hasher)
        {
            _authRepository = authRepository;
            _hasher = hasher;
        }

        public async Task<ApiResponse<int>> Handle(
            RevokeOtherSessionsCommand request,
            CancellationToken cancellationToken)
        {
            var currentSessionId = await CurrentSessionResolver.ResolveAsync(
                _authRepository,
                _hasher,
                request.UserId,
                request.RefreshToken,
                cancellationToken);

            var revoked = await _authRepository.RevokeOtherSessionsAsync(
                request.UserId,
                currentSessionId,
                cancellationToken);

            return new ApiResponse<int>
            {
                Success = true,
                Data = revoked
            };
        }
    }
}
