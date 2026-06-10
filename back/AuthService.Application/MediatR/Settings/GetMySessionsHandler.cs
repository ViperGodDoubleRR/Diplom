using AuthService.Application.DTO;
using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;

namespace AuthService.Application.MediatR.Settings
{
    public class GetMySessionsHandler
        : IRequestHandler<GetMySessionsQuery, ApiResponse<List<SessionDto>>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHasher _hasher;

        public GetMySessionsHandler(IAuthRepository authRepository, IHasher hasher)
        {
            _authRepository = authRepository;
            _hasher = hasher;
        }

        public async Task<ApiResponse<List<SessionDto>>> Handle(
            GetMySessionsQuery request,
            CancellationToken cancellationToken)
        {
            var sessions = await _authRepository.GetActiveSessionsByUserIdAsync(
                request.UserId,
                cancellationToken);

            var currentSessionId = await CurrentSessionResolver.ResolveAsync(
                _authRepository,
                _hasher,
                request.UserId,
                request.RefreshToken,
                cancellationToken);

            return new ApiResponse<List<SessionDto>>
            {
                Success = true,
                Data = sessions.Select(x => new SessionDto
                {
                    Id = x.Id,
                    DeviceInfo = x.DeviceInfo,
                    IpAddress = x.IpAddress,
                    CreatedAt = x.CreatedAt,
                    ExpiresAt = x.ExpiresAt,
                    IsCurrent = currentSessionId.HasValue && x.Id == currentSessionId.Value
                }).ToList()
            };
        }
    }
}
