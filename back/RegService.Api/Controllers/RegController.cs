//{====================================================================}
//{ Модуль AuthController.cs}
//{ гр.П41 }
//{ Разработчик: Куприянович А.П }
//{ Модифицирован: 27.05.2026 }
//{ --------------------------------------------------------------------}
//{модуль для регистрации пользователей
//{ ********************************************************************}  
using MediatR;


using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using RegService.Api.DTO;
using RegService.Application.Mediatr.CheckCode;
using RegService.Application.Mediatr.RegisteredUser;
using RegService.Application.Mediatr.SendEmail;

using Shared.Application.Contracts;
using Shared.RabbitMQ.rpc.Abstraction;

namespace RegService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class RegController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RegController> _logger;
        public RegController(IMediator mediator, ILogger<RegController> logger)
        { 
            _mediator = mediator;
            _logger = logger;
        }
        [HttpGet("send-email-code")]
        public async Task<IActionResult> SendEmailCode(string email)
        {
            var command = new SendEmailCommandCode(email);
            ApiResponse<string> response = await _mediator.Send(command);
            _logger.LogInformation("Ответ клиенту: " ,response);
            return Ok(response);
        }
        [HttpGet("check-code")] 
        public async Task<IActionResult> CheckCode(string email,string code)
        {
            _logger.LogInformation($"Email:{email} /n Code: {code}");
            var command = new CheckCodeCommand(email, code);
            ApiResponse<string> response = await _mediator.Send(command);
            _logger.LogInformation("Проверка кода: ", response);
            return Ok(response);    
        }
        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTO user)
        {
                _logger.LogInformation(user.ToString());
                var command = new RegisterUserCommand(user.email, user.login, user.password);
                var response = await _mediator.Send(command);
                 _logger.LogInformation("CallBack: ",response);
                return Ok(response);
        }
    }
}
