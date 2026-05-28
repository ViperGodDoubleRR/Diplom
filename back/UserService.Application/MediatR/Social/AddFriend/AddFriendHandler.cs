using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.AddFriend
{
    public class AddFriendHandler
        : IRequestHandler<
            AddFriendCommand,
            ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISocialRepository _socialRepository;

        public AddFriendHandler(
            IUserRepository userRepository,
            ISocialRepository socialRepository)
        {
            _userRepository = userRepository;
            _socialRepository = socialRepository;
        }

        public async Task<ApiResponse<bool>> Handle(
            AddFriendCommand request,
            CancellationToken cancellationToken)
        {
            // нельзя добавить себя
            if (request.MyId == request.FriendId)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "SELF_FRIEND",
                        Message = "You cannot add yourself"
                    }
                };
            }

            // существует ли юзер
            var friend =
                await _userRepository.GetByIdAsync(
                    request.FriendId);

            if (friend is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "USER_NOT_FOUND",
                        Message = "User not found"
                    }
                };
            }

            // уже есть?
            var exists =
                await _socialRepository.IsFriendExistsAsync(
                    request.MyId,
                    request.FriendId);

            if (exists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "ALREADY_FRIEND",
                        Message = "Already in friends"
                    }
                };
            }

            var entity = new FriendList
            {
                MyId = request.MyId,
                FriendId = request.FriendId,
                ChangeLogin = friend.Login
            };

            await _socialRepository.AddFriendAsync(entity);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}
