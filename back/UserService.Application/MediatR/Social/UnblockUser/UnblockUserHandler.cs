using MediatR;

using Shared.Application.Contracts;

using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.UnblockUser
{
    public class UnblockUserHandler : IRequestHandler<UnblockUserCommand, ApiResponse<bool>>
    {
        private readonly ISocialRepository _socialRepository;

        public UnblockUserHandler(ISocialRepository socialRepository)
        {
            _socialRepository = socialRepository;
        }

        public async Task<ApiResponse<bool>> Handle(
            UnblockUserCommand request,
            CancellationToken cancellationToken)
        {
            var block = await _socialRepository.GetBlockAsync(
                request.MyId,
                request.BlackId,
                cancellationToken);

            if (block is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "BLOCK_NOT_FOUND",
                        Message = "Пользователь не найден в чёрном списке"
                    }
                };
            }

            await _socialRepository.RemoveBlockAsync(block, cancellationToken);

            return new ApiResponse<bool> { Success = true, Data = true };
        }
    }
}
