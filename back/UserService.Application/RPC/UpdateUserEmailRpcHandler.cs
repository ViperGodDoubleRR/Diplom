using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.UpdateUserEmail;

using UserService.Domain.IRepository;

namespace UserService.Application.RPC
{
    public class UpdateUserEmailRpcHandler
        : IRPCHandle<UpdateUserEmailRpcRequest, UpdateUserEmailRpcResponse>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserEmailRpcHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UpdateUserEmailRpcResponse> Handle(UpdateUserEmailRpcRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user is null)
            {
                return new UpdateUserEmailRpcResponse
                {
                    Success = false,
                    ErrorCode = "USER_NOT_FOUND",
                    ErrorMessage = "Пользователь не найден"
                };
            }

            user.Email = request.Email.Trim().ToLowerInvariant();
            await _userRepository.UpdateAsync(user);

            return new UpdateUserEmailRpcResponse { Success = true };
        }
    }
}
