using RegService.Domain.IRepository;

using Shared.Application.Validation;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.EmailExists;

namespace RegService.Application.RPC
{
    public class EmailExistsRpcHandler
        : IRPCHandle<EmailExistsRpcRequest, EmailExistsRpcResponse>
    {
        private readonly IRegRepository _regRepository;

        public EmailExistsRpcHandler(IRegRepository regRepository)
        {
            _regRepository = regRepository;
        }

        public async Task<EmailExistsRpcResponse> Handle(EmailExistsRpcRequest request)
        {
            var email = InputValidator.NormalizeEmail(request.Email);

            return new EmailExistsRpcResponse
            {
                Exists = await _regRepository.EmailExistsAsync(email, request.ExcludeUserId)
            };
        }
    }
}
