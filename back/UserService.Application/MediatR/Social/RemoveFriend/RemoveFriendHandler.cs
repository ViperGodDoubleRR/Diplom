using MediatR;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.RemoveFriend
{
    public class RemoveFriendHandler : IRequestHandler<RemoveFriendCommand, ApiResponse<bool>>
    {
        private readonly ISocialRepository _socialRepository;

        public RemoveFriendHandler(ISocialRepository socialRepository)
        {
            _socialRepository = socialRepository;
        }

        public async Task<ApiResponse<bool>> Handle(
            RemoveFriendCommand request,
            CancellationToken cancellationToken)
        {
            var friend = await _socialRepository.GetFriendAsync(
                request.MyId,
                request.FriendId,
                cancellationToken);

            if (friend is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "FRIEND_NOT_FOUND",
                        Message = "Пользователь не найден в друзьях"
                    }
                };
            }

            await _socialRepository.RemoveFriendAsync(friend, cancellationToken);

            return new ApiResponse<bool> { Success = true, Data = true };
        }
    }
}
