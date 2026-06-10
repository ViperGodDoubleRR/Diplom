using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.BlockUser
{
    public class BlockUserHandler : IRequestHandler<BlockUserCommand, ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISocialRepository _socialRepository;
        private readonly ILogger<BlockUserHandler> _logger;

        public BlockUserHandler(
            IUserRepository userRepository,
            ISocialRepository socialRepository,
            ILogger<BlockUserHandler> logger)
        {
            _userRepository = userRepository;
            _socialRepository = socialRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(
            BlockUserCommand request,
            CancellationToken cancellationToken)
        {
            if (request.MyId == request.BlackId)
                return Fail("SELF_BLOCK", "Нельзя заблокировать себя");

            var user = await _userRepository.GetByIdAsync(request.BlackId, cancellationToken);

            if (user is null)
                return Fail("USER_NOT_FOUND", "Пользователь не найден");

            if (await _socialRepository.IsBlockedExistsAsync(
                    request.MyId,
                    request.BlackId,
                    cancellationToken))
            {
                return Fail("ALREADY_BLOCKED", "Пользователь уже заблокирован");
            }

            var friendship = await _socialRepository.GetFriendAsync(
                request.MyId,
                request.BlackId,
                cancellationToken);

            if (friendship is not null)
                await _socialRepository.RemoveFriendAsync(friendship, cancellationToken);

            await _socialRepository.AddBlockAsync(new BlackList
            {
                MyId = request.MyId,
                BlackId = request.BlackId
            }, cancellationToken);

            _logger.LogInformation(
                "User {MyId} blocked user {BlackId}",
                request.MyId,
                request.BlackId);

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
