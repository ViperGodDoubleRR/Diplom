using System.Security.Cryptography;

using AuthService.Domain.Interface;
using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;
using Shared.Application.Contracts.AuthJWT;
using Shared.Application.Interfaces;
using Shared.Application.Validation;

namespace AuthService.Application.MediatR.AuthGo
{
    public class AuthGoHandler : IRequestHandler<AuthGoCommand, ApiResponse<AuthGoResponse>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHasher _hasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthGoHandler> _logger;

        public AuthGoHandler(
            IAuthRepository authRepository,
            IHasher hasher,
            IJwtProvider jwtProvider,
            IConfiguration configuration,
            ILogger<AuthGoHandler> logger)
        {
            _authRepository = authRepository;
            _hasher = hasher;
            _jwtProvider = jwtProvider;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthGoResponse>> Handle(
            AuthGoCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                return Fail("EMAIL_REQUIRED", "Email обязателен");

            if (!InputValidator.IsValidEmail(command.Email))
                return Fail("INVALID_EMAIL", "Некорректный формат email");

            if (string.IsNullOrWhiteSpace(command.Password))
                return Fail("PASSWORD_REQUIRED", "Пароль обязателен");

            if (string.IsNullOrWhiteSpace(command.Code))
                return Fail("CODE_REQUIRED", "Код подтверждения обязателен");

            var email = InputValidator.NormalizeEmail(command.Email);
            var code = InputValidator.NormalizeCode(command.Code);

            var user = await _authRepository.GetUserByEmail(email, cancellationToken);
            if (user == null)
                return Fail("USER_NOT_FOUND", "Пользователь с таким email не найден");

            if (!_hasher.Verify(command.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password attempt for {Email}", email);
                return Fail("INVALID_PASSWORD", "Неверный пароль");
            }

            var isCodeValid = await _authRepository.CheckCode(email, code, cancellationToken);
            if (!isCodeValid)
                return Fail("INVALID_CODE", "Неверный или просроченный код подтверждения");

            var accessToken = _jwtProvider.GenerateAccessToken(
                user.Id,
                user.Email,
                user.Login);

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenHash = _hasher.Hash(refreshToken);

            var refreshLifetimeDays = int.Parse(
                _configuration["Jwt:RefreshTokenLifetimeDays"] ?? "30");

            await _authRepository.CreateSession(
                user.Id,
                refreshTokenHash,
                null,
                command.DeviceInfo,
                command.IpAddress,
                DateTime.UtcNow.AddDays(refreshLifetimeDays),
                cancellationToken);

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

        private static ApiResponse<AuthGoResponse> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
