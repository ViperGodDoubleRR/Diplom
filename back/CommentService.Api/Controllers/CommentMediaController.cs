using CommentService.Api.Extensions;
using CommentService.Application.DTO;
using CommentService.Application.MediatR.DeleteCommentMedia;
using CommentService.Application.MediatR.UploadCommentMedia;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("")]
    public class CommentMediaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommentMediaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{commentId:guid}/media")]
        public async Task<IActionResult> UploadCommentMedia(
            Guid commentId,
            [FromForm] UploadCommentMediaRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new UploadCommentMediaCommand
            {
                CommentId = commentId,
                UserId = userId.Value,
                File = request.File,
                MediaType = request.MediaType
            });

            return Ok(result);
        }

        [HttpDelete("{commentId:guid}/media")]
        public async Task<IActionResult> DeleteCommentMedia(Guid commentId)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new DeleteCommentMediaCommand
            {
                CommentId = commentId,
                UserId = userId.Value
            });

            return Ok(result);
        }
    }
}
