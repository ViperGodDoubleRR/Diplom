
using MediatR;

using RegService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.CreateUser;

namespace RegService.Application.Mediatr.RegisteredUser
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ApiResponse<string>>
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
        public async Task<ApiResponse<string>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            ApiResponse<string> back = new ApiResponse<string>();
            if (command.Password!=null)
            {
                var call = new CreateUserRpcRequest
                {
                    Email = command.Email,
                    Password = command.Password,
                    Login=command.Login
                };
                CreateUserRpcResponse response = await _rpcclient
                    .CallAsync<CreateUserRpcRequest, CreateUserRpcResponse>("auth.rpc", call);
                
                var IsCreate = await _regRepository.RegisterUser(response.Id,response.Email,response.Login,cancellationToken);
                if (IsCreate)
                {
                    back.Success = true;
                    back.Data = "Пользователь успешно зарегистрирован!";
                }
                else
                {
                    back.Success= false;
                    back.Error = new ApiError
                    {
                        Code ="IS_CONFIRMEMAIL_EXPIRED",
                        Message = "Вы долго регистрировали пользователя,повторите заново.У вас будет 10 минут для потверждения пользователя с вашим Email",
                    };
                }
            }
                return back;
        }
    }
}
