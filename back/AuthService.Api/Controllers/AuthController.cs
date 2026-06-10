using AuthService.Api.DTO;
using AuthService.Application.MediatR.AuthGo;
using AuthService.Application.MediatR.AuthRequestCode;
using AuthService.Application.MediatR.RefreshToken;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Application.Contracts;
using Shared.Application.Contracts.AuthJWT;

namespace AuthService.Api.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("auth-request-code")]
        public async Task<IActionResult> RequestCode([FromQuery] string email)
        {
            var response = await _mediator.Send(new AuthRequestCodeCommand(email));
            _logger.LogInformation("auth-request-code: Success={Success}", response.Success);
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
                ipAddress);

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var response = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken));
            _logger.LogInformation("refresh: Success={Success}", response.Success);
            return Ok(response);
        }
    }
}
