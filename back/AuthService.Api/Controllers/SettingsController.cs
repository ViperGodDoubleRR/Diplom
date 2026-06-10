using AuthService.Api.DTO;
using AuthService.Api.Extensions;
using AuthService.Application.MediatR.Settings;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions([FromHeader(Name = "X-Refresh-Token")] string? refreshToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var response = await _mediator.Send(new GetMySessionsQuery
            {
                UserId = userId.Value,
                RefreshToken = refreshToken
            });

            return Ok(response);
        }

        [HttpDelete("sessions/{sessionId:int}")]
        public async Task<IActionResult> RevokeSession(
            int sessionId,
            [FromBody] RevokeSessionRequest? request,
            [FromHeader(Name = "X-Refresh-Token")] string? refreshTokenHeader)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var response = await _mediator.Send(new RevokeSessionCommand
            {
                UserId = userId.Value,
                SessionId = sessionId,
                RefreshToken = request?.RefreshToken ?? refreshTokenHeader
            });

            return Ok(response);
        }

        [HttpPost("sessions/revoke-others")]
        public async Task<IActionResult> RevokeOtherSessions([FromBody] SessionTokenRequest? request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var response = await _mediator.Send(new RevokeOtherSessionsCommand
            {
                UserId = userId.Value,
                RefreshToken = request?.RefreshToken
            });

            return Ok(response);
        }

        [HttpPost("change-email/request")]
        public async Task<IActionResult> RequestChangeEmail([FromBody] ChangeEmailRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var response = await _mediator.Send(new RequestChangeEmailCommand
            {
                UserId = userId.Value,
                NewEmail = request.NewEmail,
                CurrentPassword = request.CurrentPassword
            });

            return Ok(response);
        }

        [HttpPost("change-email/confirm")]
        public async Task<IActionResult> ConfirmChangeEmail([FromBody] ChangeEmailConfirmRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var response = await _mediator.Send(new ConfirmChangeEmailCommand
            {
                UserId = userId.Value,
                NewEmail = request.NewEmail,
                Code = request.Code,
                CurrentPassword = request.CurrentPassword
            });

            return Ok(response);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordSettingsRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var response = await _mediator.Send(new ChangePasswordCommand
            {
                UserId = userId.Value,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword,
                RefreshToken = request.RefreshToken
            });

            return Ok(response);
        }
    }
}
