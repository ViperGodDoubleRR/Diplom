using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using AuthService.Application.MediatR.AuthRequestCode;
using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Contracts.AuthJWT;
using Shared.Application.Interfaces;

namespace AuthService.Application.MediatR.AuthGo
{
    public class AuthGoHandler
     : IRequestHandler<AuthGoCommand, ApiResponse<AuthGoResponse>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHasher _hasher;
        private readonly IJwtProvider _jwtProvider;

        public AuthGoHandler(IAuthRepository authRepository,IHasher hasher,IJwtProvider jwtProvider)
        {
            _authRepository = authRepository;
            _hasher = hasher;
            _jwtProvider = jwtProvider;
        }

        public async Task<ApiResponse<AuthGoResponse>> Handle(AuthGoCommand command,CancellationToken cancellationToken)
        {
            // 1. проверка кода
            var isCodeValid = await _authRepository
                .CheckCode(command.Email, command.Code, cancellationToken);

            if (!isCodeValid)
                return Fail("INVALID_CODE", "Некорректный код");

            // 2. user
            var user = await _authRepository
                .GetUserByEmail(command.Email, cancellationToken);

            if (user == null)
                return Fail("USER_NOT_FOUND", "Пользователь не найден");

            // 3. password
            if (!_hasher.Verify(command.Password, user.PasswordHash))
                return Fail("INVALID_PASSWORD", "Неверный пароль");

            // 4. access token
            var accessToken = _jwtProvider.GenerateAccessToken(
                user.Id,
                user.Email,
                user.Login);

            // 5. refresh token
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenHash = _hasher.Hash(refreshToken);
            // 6. сохранить сессию (в БД можно хранить HASH — лучше)
            await _authRepository.CreateSession(
                user.Id,
                refreshTokenHash,
                command.DeviceInfo,
                command.IpAddress,
                cancellationToken);

            // 7. response
            return new ApiResponse<AuthGoResponse>
            {
                Success = true,
                Data = new AuthGoResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }
            };
        }

        private ApiResponse<AuthGoResponse> Fail(string code, string message)
        {
            return new ApiResponse<AuthGoResponse>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = code,
                    Message = message
                }
            };
        }
    }
}