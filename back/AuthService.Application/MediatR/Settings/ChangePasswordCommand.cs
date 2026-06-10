using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.Settings
{
    public class ChangePasswordCommand : IRequest<ApiResponse<string>>
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
    }
}
