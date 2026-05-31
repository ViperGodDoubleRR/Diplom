//{====================================================================}
//{ Модуль AuthController.cs}
//{ гр.П41 }
//{ Разработчик: Куприянович А.П }
//{ Модифицирован: 27.05.2026 }
//{ --------------------------------------------------------------------}
//{модуль для восстановления пароля пользователя
//{ ********************************************************************}  

using AuthService.Application.MediatR.ResCheckCode;
using AuthService.Application.MediatR.ResPassword;
using AuthService.Application.MediatR.ResRequestCode;

using MediatR;

using Microsoft.AspNetCore.Mvc;


using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using AuthService.Api.DTO;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class ResController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ResController> _logger;
        public ResController(IMediator mediator, ILogger<ResController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpGet("res-request-code")]
        public async Task<IActionResult> RequestCode(string email)
        {
            var command = new ResRequestCodeCommand(email);
            ApiResponse<string> response = await _mediator.Send(command);
            _logger.LogInformation("Ответ клиенту: {@response}", response);
            return Ok(response);
        }
        [HttpGet("res-check-code")]
        public async Task<IActionResult> ResCheckCode(string email,string code)
        {
            var command = new ResCheckCodeCommand(email,code);
            ApiResponse<string> response = await _mediator.Send(command);
            _logger.LogInformation("Ответ клиенту: {@response}",response);
            return Ok(response);
        }
        [HttpPost("res-change-password")]
        public async Task<IActionResult> ResChangePassword([FromBody] ChangePasswordRequest request)
        {
            var command = new ResPasswordCommand(request.Email, request.Password);
            ApiResponse<string> response = await _mediator.Send(command);
            _logger.LogInformation("Ответ клиенту: {@response}", response);
            return Ok(response);
        }
    }
}
