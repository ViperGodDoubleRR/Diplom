using MediatR;

using Shared.Application.Contracts;

using UserService.Application.DTO;

namespace UserService.Application.MediatR.UpdateUser
{
    public class UpdateUserCommand : IRequest<ApiResponse<UserProfileDto>>
    {
        public Guid UserId { get; set; }
        public UpdateUserDto Dto { get; set; } = null!;
    }
}
