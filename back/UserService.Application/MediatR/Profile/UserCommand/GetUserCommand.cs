using MediatR;

using Shared.Application.Contracts;

using UserService.Application.DTO;

namespace UserService.Application.MediatR.UserCommand
{
    public class GetUserCommand : IRequest<ApiResponse<UserProfileDto>>
    {
        public Guid UserId { get; set; }
    }
}
