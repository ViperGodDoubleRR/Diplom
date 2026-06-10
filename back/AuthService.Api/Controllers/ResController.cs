using AuthService.Api.DTO;
using AuthService.Application.MediatR.ResCheckCode;
using AuthService.Application.MediatR.ResPassword;
using AuthService.Application.MediatR.ResRequestCode;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Shared.Application.Contracts;

namespace AuthService.Api.Controllers
{
    [ApiController]
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
        public async Task<IActionResult> RequestCode([FromQuery] string email)
        {
            var response = await _mediator.Send(new ResRequestCodeCommand(email));
            _logger.LogInformation("res-request-code: Success={Success}", response.Success);
            return Ok(response);
        }

        [HttpGet("res-check-code")]
        public async Task<IActionResult> ResCheckCode([FromQuery] string email, [FromQuery] string code)
        {
            var response = await _mediator.Send(new ResCheckCodeCommand(email, code));
            _logger.LogInformation("res-check-code: Success={Success}", response.Success);
            return Ok(response);
        }

        [HttpPost("res-change-password")]
        public async Task<IActionResult> ResChangePassword([FromBody] ChangePasswordRequest request)
        {
            var command = new ResPasswordCommand(
                request.Email,
                request.Password,
                request.ResetToken);

            var response = await _mediator.Send(command);
            _logger.LogInformation("res-change-password: Success={Success}", response.Success);
            return Ok(response);
        }
    }
}
