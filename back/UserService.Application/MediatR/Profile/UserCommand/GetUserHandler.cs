using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;

using UserService.Domain.IRepository;
using UserService.Domain.Models;
namespace UserService.Application.MediatR.UserCommand
{
    public class GetUserHandler
    : IRequestHandler<GetUserCommand, ApiResponse<User>>
    {
        private readonly IUserRepository _userRepository;

        public GetUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<User>> Handle(GetUserCommand command,CancellationToken cancellationToken)
        {
            var apiResponse = new ApiResponse<User>();

            var user =
                await _userRepository.GetByIdAsync(command.UserId);

            if (user is null)
            {
                apiResponse.Success = false;

                apiResponse.Error = new ApiError
                {
                    Code = "USER_NOT_FOUND",
                    Message = "Пользователь не найден"
                };

                return apiResponse;
            }

            apiResponse.Success = true;
            apiResponse.Data = user;

            return apiResponse;
        }
    }
}
