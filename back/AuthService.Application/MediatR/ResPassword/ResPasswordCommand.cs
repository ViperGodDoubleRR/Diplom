using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.ResPassword
{
    public class ResPasswordCommand : IRequest<ApiResponse<string>>
    {
        public string Email { get; }
        public string NewPassword { get; }
        public string ResetToken { get; }

        public ResPasswordCommand(string email, string newPassword, string resetToken)
        {
            Email = email;
            NewPassword = newPassword;
            ResetToken = resetToken;
        }
    }
}
