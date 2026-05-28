using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.UpdateUser
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, ApiResponse<User>>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<User>> Handle(UpdateUserCommand command,CancellationToken cancellationToken)
        {
            var response = new ApiResponse<User>();

            var user =
                await _userRepository.GetByIdAsync(command.UserId);

            if (user is null)
            {
                response.Success = false;

                response.Error = new ApiError
                {
                    Code = "USER_NOT_FOUND",
                    Message = "Пользователь не найден"
                };

                return response;
            }

            user.Login = command.Dto.Login;
            user.Tag = command.Dto.Tag;
            user.Description = command.Dto.Description;

            await _userRepository.UpdateAsync(user);

            response.Success = true;
            response.Data = user;

            return response;
        }
    }
}
