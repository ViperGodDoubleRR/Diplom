using Shared.Application.Validation;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.EmailExists;

using UserService.Domain.IRepository;

namespace UserService.Application.RPC
{
    public class EmailExistsRpcHandler
        : IRPCHandle<EmailExistsRpcRequest, EmailExistsRpcResponse>
    {
        private readonly IUserRepository _userRepository;

        public EmailExistsRpcHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<EmailExistsRpcResponse> Handle(EmailExistsRpcRequest request)
        {
            var email = InputValidator.NormalizeEmail(request.Email);

            return new EmailExistsRpcResponse
            {
                Exists = await _userRepository.EmailExistsAsync(email, request.ExcludeUserId)
            };
        }
    }
}
