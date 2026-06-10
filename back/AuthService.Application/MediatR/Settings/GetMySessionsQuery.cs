using AuthService.Application.DTO;

using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.Settings
{
    public class GetMySessionsQuery : IRequest<ApiResponse<List<SessionDto>>>
    {
        public Guid UserId { get; set; }
        public string? RefreshToken { get; set; }
    }
}
