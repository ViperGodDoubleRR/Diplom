using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.Settings
{
    public class RevokeSessionResult
    {
        public bool Revoked { get; set; }
        public bool WasCurrentSession { get; set; }
    }

    public class RevokeSessionCommand : IRequest<ApiResponse<RevokeSessionResult>>
    {
        public Guid UserId { get; set; }
        public int SessionId { get; set; }
        public string? RefreshToken { get; set; }
    }
}
