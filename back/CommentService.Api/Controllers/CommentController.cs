using CommentService.Api.Extensions;
using CommentService.Application.DTO;
using CommentService.Application.MediatR.AddCommentReaction;
using CommentService.Application.MediatR.CreateComment;
using CommentService.Application.MediatR.DeleteComment;
using CommentService.Application.MediatR.GetCommentReplies;
using CommentService.Application.MediatR.GetPostComments;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("")]
    public class CommentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new CreateCommentCommand
            {
                PostId = request.PostId,
                ParentId = request.ParentId,
                UserId = userId.Value,
                Text = request.Text
            });

            return Ok(result); 
        }

        [HttpGet("post/{postId:guid}")]
        public async Task<IActionResult> GetPostComments(
            Guid postId,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 50)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetPostCommentsQuery
            {
                PostId = postId,
                CurrentUserId = userId.Value,
                Offset = offset,
                Limit = limit
            });

            return Ok(result);
        }

        [HttpGet("{parentCommentId:guid}/replies")]
        public async Task<IActionResult> GetCommentReplies(
            Guid parentCommentId,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 3)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetCommentRepliesQuery
            {
                ParentCommentId = parentCommentId,
                CurrentUserId = userId.Value,
                Offset = offset,
                Limit = limit
            });

            return Ok(result);
        }

        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new DeleteCommentCommand
            {
                CommentId = commentId,
                UserId = userId.Value
            });

            return Ok(result);
        }

        [HttpPost("{commentId:guid}/reaction")]
        public async Task<IActionResult> AddReaction(
            Guid commentId,
            [FromBody] AddCommentReactionRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new AddCommentReactionCommand
            {
                CommentId = commentId,
                UserId = userId.Value,
                Type = request.Type
            });

            return Ok(result);
        }
    }
}
