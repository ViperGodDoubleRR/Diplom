    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Shared.Application.Interfaces;
    using AuthService.Domain.Interface;

    using Shared.RabbitMQ.rpc.Abstraction;
    using Shared.RabbitMQ.rpc.Contracts.CreateUser;

    namespace AuthService.Application.Events
    {
        public class CreateUser : IRPCHandle<CreateUserRpcRequest,CreateUserRpcResponse>
        {
            private readonly IAuthRepository _authRepository;
            private readonly IHasher _hasher;
            public CreateUser(IAuthRepository authRepository,IHasher hasher)
            {
                 _hasher = hasher;
                _authRepository = authRepository;
            }
            public async Task<CreateUserRpcResponse>Handle(CreateUserRpcRequest request)
            {
                request.Password = _hasher.Hash(request.Password);
                var id = await _authRepository.CreateUser(request.Email, request.Login, request.Password);
                return new CreateUserRpcResponse
                {
                    Id = id,
                    Email = request.Email,
                    Login = request.Login,
                };
            }
        }
    }
