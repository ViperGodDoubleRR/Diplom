using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.BlockUser
{
    public class BlockUserHandler
        : IRequestHandler<
            BlockUserCommand,
            ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISocialRepository _socialRepository;

        public BlockUserHandler(
            IUserRepository userRepository,
            ISocialRepository socialRepository)
        {
            _userRepository = userRepository;
            _socialRepository = socialRepository;
        }

        public async Task<ApiResponse<bool>> Handle(
            BlockUserCommand request,
            CancellationToken cancellationToken)
        {
            if (request.MyId == request.BlackId)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "SELF_BLOCK",
                        Message = "You cannot block yourself"
                    }
                };
            }

            var user =
                await _userRepository.GetByIdAsync(
                    request.BlackId);

            if (user is null)
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

            var exists =
                await _socialRepository.IsBlockedExistsAsync(
                    request.MyId,
                    request.BlackId);

            if (exists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "ALREADY_BLOCKED",
                        Message = "User already blocked"
                    }
                };
            }

            var entity = new BlackList
            {
                MyId = request.MyId,
                BlackId = request.BlackId
            };

            await _socialRepository.AddBlockAsync(entity);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}
