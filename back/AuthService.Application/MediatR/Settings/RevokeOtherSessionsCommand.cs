using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.Settings
{
    public class RevokeOtherSessionsCommand : IRequest<ApiResponse<int>>
    {
        public Guid UserId { get; set; }
        public string? RefreshToken { get; set; }
    }
}
