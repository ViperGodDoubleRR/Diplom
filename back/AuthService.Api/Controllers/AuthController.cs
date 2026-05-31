//{====================================================================}
//{ Модуль AuthController.cs}
//{ гр.П41 }
//{ Разработчик: Куприянович А.П }
//{ Модифицирован: 27.05.2026 }
//{ --------------------------------------------------------------------}
//{модуль для работы аутентификации пользователя а так же выдачи токена
//{ ********************************************************************}  



using AuthService.Api.DTO;
using AuthService.Application.MediatR.AuthGo;
using AuthService.Application.MediatR.AuthRequestCode;
using AuthService.Application.MediatR.RefreshToken;
using AuthService.Application.MediatR.ResCheckCode;
using AuthService.Application.MediatR.ResRequestCode;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Application.Contracts;
using Shared.Application.Contracts.AuthJWT;

namespace Auth.Api.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator=mediator;
            _logger=logger;
        }
        [HttpGet("auth-request-code")]
        public async Task<IActionResult> RequestCode(string email)
        {
            var command = new AuthRequestCodeCommand(email);
            ApiResponse<string> response = await _mediator.Send(command);
            _logger.LogInformation("Ответ клиенту: {@response}", response);
            return Ok(response);
        }
        [HttpPost("authorized-user")]
        public async Task<IActionResult> AuthorizedUser([FromBody] AuthGoRequest request)
        {
            var ipAddress =
                HttpContext.Request.Headers["X-Forwarded-For"]
                    .FirstOrDefault()?
                    .Split(',')
                    .FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown";

            var command = new AuthGoCommand(
                request.Email,
                request.Password,
                request.Code,
                request.DeviceInfo,
                ipAddress
            );

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("🔄 REFRESH REQUEST: refreshToken={Token}", request.RefreshToken);

            var command = new RefreshTokenCommand(request.RefreshToken);
            var response = await _mediator.Send(command);
                
            _logger.LogInformation("🔄 REFRESH RESPONSE: {@Response}", response);

            return Ok(response);
        }
    }
}
