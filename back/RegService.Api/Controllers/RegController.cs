using MediatR;

using Microsoft.AspNetCore.Mvc;

using RegService.Api.DTO;
using RegService.Application.Mediatr.CheckCode;
using RegService.Application.Mediatr.RegisteredUser;
using RegService.Application.Mediatr.SendEmail;

using Shared.Application.Contracts;

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
        public async Task<IActionResult> SendEmailCode([FromQuery] string email)
        {
            var response = await _mediator.Send(new SendEmailCommandCode(email));
            _logger.LogInformation("send-email-code: Success={Success}", response.Success);
            return Ok(response);
        }

        [HttpGet("check-code")]
        public async Task<IActionResult> CheckCode([FromQuery] string email, [FromQuery] string code)
        {
            var response = await _mediator.Send(new CheckCodeCommand(email, code));
            _logger.LogInformation("check-code: Success={Success}", response.Success);
            return Ok(response);
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTO user)
        {
            var command = new RegisterUserCommand(user.email, user.login, user.password);
            var response = await _mediator.Send(command);
            _logger.LogInformation("register-user: Success={Success}", response.Success);
            return Ok(response);
        }
    }
}
