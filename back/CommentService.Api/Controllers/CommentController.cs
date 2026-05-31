using System.Security.Claims;

using CommentService.Application.DTO;
using CommentService.Application.MediatR.CreateComment;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class CommentController : Controller
    {
        private readonly IMediator _mediator;
        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [Authorize]
        [HttpPost("comment")]
        public async Task<IActionResult> CreateComment(
    [FromBody] CreateCommentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var parsedUserId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest();

            var command = new CreateCommentCommand
            {
                PostId = request.PostId,
                ParentId = request.ParentId,
                UserId = parsedUserId,
                Text = request.Text
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
