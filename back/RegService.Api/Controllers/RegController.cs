using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using RegService.Api.DTO;
using RegService.Application.Mediatr.CheckCode;
using RegService.Application.Mediatr.RegisteredUser;
using RegService.Application.Mediatr.SendEmail;

using Shared.RabbitMQ.rpc.Abstraction;

namespace RegService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            var response = await _mediator.Send(command);
            _logger.LogInformation(response);
            return Ok(response);
        }
        [HttpGet("check-code")] 
        public async Task<IActionResult> CheckCode(string email,string code)
        {
            _logger.LogInformation($"Email:{email} /n Code: {code}");
            var command = new CheckCodeCommand(email, code);
            var response = await _mediator.Send(command);
            return Ok(response);    
        }
        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTO user)
        {

                
                
                var command = new RegisterUserCommand(user.email, user.login, user.password);
                var response = await _mediator.Send(command);
                 _logger.LogInformation(response);
                return Ok(response);
        }
    }
}
