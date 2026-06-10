using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.Settings
{
    public class RequestChangeEmailCommand : IRequest<ApiResponse<string>>
    {
        public Guid UserId { get; set; }
        public string NewEmail { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
