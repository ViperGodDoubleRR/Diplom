using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.AddFriend
{
    public class AddFriendHandler : IRequestHandler<AddFriendCommand, ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISocialRepository _socialRepository;
        private readonly ILogger<AddFriendHandler> _logger;

        public AddFriendHandler(
            IUserRepository userRepository,
            ISocialRepository socialRepository,
            ILogger<AddFriendHandler> logger)
        {
            _userRepository = userRepository;
            _socialRepository = socialRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(
            AddFriendCommand request,
            CancellationToken cancellationToken)
        {
            if (request.MyId == request.FriendId)
                return Fail("SELF_FRIEND", "Нельзя добавить себя в друзья");

            var friend = await _userRepository.GetByIdAsync(request.FriendId, cancellationToken);

            if (friend is null)
                return Fail("USER_NOT_FOUND", "Пользователь не найден");

            if (await _socialRepository.IsBlockedBetweenAsync(
                    request.MyId,
                    request.FriendId,
                    cancellationToken))
            {
                return Fail("USER_BLOCKED", "Невозможно добавить этого пользователя");
            }

            if (await _socialRepository.IsFriendExistsAsync(
                    request.MyId,
                    request.FriendId,
                    cancellationToken))
            {
                return Fail("ALREADY_FRIEND", "Пользователь уже в друзьях");
            }

            await _socialRepository.AddFriendAsync(new FriendList
            {
                MyId = request.MyId,
                FriendId = request.FriendId,
                ChangeLogin = friend.Login
            }, cancellationToken);

            _logger.LogInformation(
                "User {MyId} added friend {FriendId}",
                request.MyId,
                request.FriendId);

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
