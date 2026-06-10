using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.Application.Validation;

namespace AuthService.Application.MediatR.ResCheckCode
{
    public class ResCheckCodeHandler : IRequestHandler<ResCheckCodeCommand, ApiResponse<string>>
    {
        private readonly IResRepository _resRepository;
        private readonly IJwtProvider _jwtProvider;

        public ResCheckCodeHandler(IResRepository resRepository, IJwtProvider jwtProvider)
        {
            _resRepository = resRepository;
            _jwtProvider = jwtProvider;
        }

        public async Task<ApiResponse<string>> Handle(
            ResCheckCodeCommand command,
            CancellationToken cancellationToken)
        {
            if (!InputValidator.IsValidEmail(command.Email))
            {
                return Fail("INVALID_EMAIL", "Некорректный email");
            }

            if (string.IsNullOrWhiteSpace(command.Code))
            {
                return Fail("INVALID_CODE", "Код обязателен");
            }

            var email = InputValidator.NormalizeEmail(command.Email);
            var isValid = await _resRepository.CheckCode(email, command.Code, cancellationToken);

            if (!isValid)
            {
                return Fail("INVALID_CODE", "Неправильный или просроченный код");
            }

            var resetToken = _jwtProvider.GeneratePasswordResetToken(email);

            return new ApiResponse<string>
            {
                Success = true,
                Data = resetToken
            };
        }

        private static ApiResponse<string> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
