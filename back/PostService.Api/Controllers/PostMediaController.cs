using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PostService.Api.Extensions;
using PostService.Application.DTO;
using PostService.Application.MediatR.Media.UploadPostMedia;

namespace PostService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("")]
    public class PostMediaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PostMediaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{postId:guid}/media")]
        public async Task<IActionResult> UploadPostMedia(
            Guid postId,
            [FromForm] UploadPostMediaRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new UploadPostMediaCommand
            {
                UserId = userId.Value,
                PostId = postId,
                File = request.File,
                MediaType = request.MediaType
            });

            return Ok(result);
        }
    }
}
