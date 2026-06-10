using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.RenameFriend
{
    public class RenameFriendHandler : IRequestHandler<RenameFriendCommand, ApiResponse<string>>
    {
        private const int MinNicknameLength = 1;
        private const int MaxNicknameLength = 32;

        private readonly ISocialRepository _socialRepository;
        private readonly ILogger<RenameFriendHandler> _logger;

        public RenameFriendHandler(
            ISocialRepository socialRepository,
            ILogger<RenameFriendHandler> logger)
        {
            _socialRepository = socialRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(
            RenameFriendCommand request,
            CancellationToken cancellationToken)
        {
            var nickname = request.Login.Trim();

            if (nickname.Length < MinNicknameLength || nickname.Length > MaxNicknameLength)
            {
                return Fail(
                    "INVALID_NICKNAME",
                    $"Имя для друга: от {MinNicknameLength} до {MaxNicknameLength} символов");
            }

            var friend = await _socialRepository.GetFriendAsync(
                request.MyId,
                request.FriendId,
                cancellationToken);

            if (friend is null)
                return Fail("FRIEND_NOT_FOUND", "Пользователь не найден в друзьях");

            friend.ChangeLogin = nickname;

            await _socialRepository.UpdateFriendAsync(friend, cancellationToken);

            _logger.LogInformation(
                "User {MyId} renamed friend {FriendId} to {Nickname}",
                request.MyId,
                request.FriendId,
                nickname);

            return new ApiResponse<string>
            {
                Success = true,
                Data = nickname
            };
        }

        private static ApiResponse<string> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
