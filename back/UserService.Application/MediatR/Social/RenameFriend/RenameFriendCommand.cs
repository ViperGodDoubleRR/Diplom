using MediatR;

using Shared.Application.Contracts;

namespace UserService.Application.MediatR.RenameFriend
{
    public class RenameFriendCommand : IRequest<ApiResponse<string>>
    {
        public Guid MyId { get; set; }
        public Guid FriendId { get; set; }
        public string Login { get; set; } = string.Empty;
    }
}
