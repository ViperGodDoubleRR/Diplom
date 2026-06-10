using MediatR;

using RegService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.Application.Validation;

namespace RegService.Application.Mediatr.CheckCode
{
    public class CheckCodeHandler : IRequestHandler<CheckCodeCommand, ApiResponse<string>>
    {
        private readonly IRegRepository _regRepository;

        public CheckCodeHandler(IRegRepository regRepository)
        {
            _regRepository = regRepository;
        }

        public async Task<ApiResponse<string>> Handle(
            CheckCodeCommand command,
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
            var isValid = await _regRepository.CheckCode(email, command.Code, cancellationToken);

            if (!isValid)
            {
                return Fail("INVALID_CODE", "Неверный или просроченный код");
            }

            return new ApiResponse<string>
            {
                Success = true,
                Data = "EMAIL_CONFIRMED"
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
