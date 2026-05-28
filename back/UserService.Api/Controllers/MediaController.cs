using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using UserService.Application.DTO;
using UserService.Application.MediatR.DeleteAllMedia;
using UserService.Application.MediatR.DeleteAvatar;
using UserService.Application.MediatR.GetMyMedia;
using UserService.Application.MediatR.ReplaceMedia;
using UserService.Application.MediatR.UploadAvatar;
namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class MediaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MediaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost("media-upload")]
        public async Task<IActionResult> UploadAvatar(
            [FromForm] UploadMediaRequest request)
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier
            );

            if (userIdClaim is null)
                return Unauthorized();
            var command = new UploadAvatarCommand
            {
                UserId = Guid.Parse(userIdClaim.Value),
                File = request.File,
                MediaType = request.MediaType
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        [Authorize]
        [HttpGet("media")]
        public async Task<IActionResult> GetMyMedia()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();
            var query = new GetMyMediaQuery
            {
                UserId = Guid.Parse(userIdClaim.Value)
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [Authorize]
        [HttpDelete("/media/{mediaId}")]
        public async Task<IActionResult> DeleteMedia(int mediaId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();
            
            var command = new DeleteMediaCommand
            {
                UserId = Guid.Parse(userIdClaim.Value),
                MediaId = mediaId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        [Authorize]
        [HttpDelete("media")]
        public async Task<IActionResult> DeleteAllMedia()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();
            var command = new DeleteAllMediaCommand
            {
                UserId = Guid.Parse(userIdClaim.Value)
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        [Authorize]
        [HttpPut("media/replace/{mediaId}")]
        public async Task<IActionResult> ReplaceMedia(int mediaId, [FromForm] ReplaceMediaRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();

            var command = new ReplaceMediaCommand
            {
                UserId = Guid.Parse(userIdClaim.Value),
                MediaId = mediaId,
                File = request.File,
                MediaType = request.MediaType
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
