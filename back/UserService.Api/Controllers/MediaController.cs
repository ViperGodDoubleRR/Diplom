using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UserService.Application.DTO;
using UserService.Application.MediatR.DeleteAllMedia;
using UserService.Application.MediatR.DeleteAvatar;
using UserService.Application.MediatR.GetMyMedia;
using UserService.Application.MediatR.ReplaceMedia;
using UserService.Application.MediatR.UploadAvatar;
using UserService.Api.Extensions;

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
        public async Task<IActionResult> UploadAvatar([FromForm] UploadMediaRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new UploadAvatarCommand
            {
                UserId = userId.Value,
                File = request.File,
                MediaType = request.MediaType
            });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("media")]
        public async Task<IActionResult> GetMyMedia()
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetMyMediaQuery { UserId = userId.Value });
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("media/{mediaId:int}")]
        public async Task<IActionResult> DeleteMedia(int mediaId)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new DeleteMediaCommand
            {
                UserId = userId.Value,
                MediaId = mediaId
            });

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("media")]
        public async Task<IActionResult> DeleteAllMedia()
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new DeleteAllMediaCommand { UserId = userId.Value });
            return Ok(result);
        }

        [Authorize]
        [HttpPut("media/replace/{mediaId:int}")]
        public async Task<IActionResult> ReplaceMedia(
            int mediaId,
            [FromForm] ReplaceMediaRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new ReplaceMediaCommand
            {
                UserId = userId.Value,
                MediaId = mediaId,
                File = request.File,
                MediaType = request.MediaType
            });

            return Ok(result);
        }
    }
}
