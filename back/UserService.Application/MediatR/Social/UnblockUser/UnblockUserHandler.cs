using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.UnblockUser
{
    public class UnblockUserHandler
        : IRequestHandler<
            UnblockUserCommand,
            ApiResponse<bool>>
    {
        private readonly ISocialRepository _socialRepository;

        public UnblockUserHandler(
            ISocialRepository socialRepository)
        {
            _socialRepository = socialRepository;
        }

        public async Task<ApiResponse<bool>> Handle(
            UnblockUserCommand request,
            CancellationToken cancellationToken)
        {
            var block =
                await _socialRepository.GetBlockAsync(
                    request.MyId,
                    request.BlackId);

            if (block is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "BLOCK_NOT_FOUND",
                        Message = "Block not found"
                    }
                };
            }

            await _socialRepository.RemoveBlockAsync(block);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}
