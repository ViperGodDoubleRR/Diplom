
using MediatR;

using RegService.Domain.IRepository;

using Shared.Application.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.CreateUser;

namespace RegService.Application.Mediatr.RegisteredUser
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, string>
    {
        private readonly IRegRepository _regRepository;
        private readonly IHasher _hasher;
        private readonly IRpcClient _rpcclient;

        public RegisterUserHandler(IRegRepository repository, IHasher hasher, IRpcClient rpcclient)
        {
            _regRepository = repository;
            _hasher = hasher;
            _rpcclient=rpcclient;
        }
        public async Task<string> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            if (command.Password!=null)
            {
                var call = new CreateUserRpcCall
                {
                    Email = command.Email,
                    Password = command.Password,
                    Login=command.Login
                };
                var response = await _rpcclient
                    .CallAsync<CreateUserRpcCall, CreateUserRpcResponse>("auth.rpc", call);
                
                var IsCreate = await _regRepository.RegisterUser(response.id,response.Email,response.Login,cancellationToken);

            }
                return "123";
        }
    }
}
