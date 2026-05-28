using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using PostService.Application.DTO;
using PostService.Application.MediatR.CreatePost;
using PostService.Application.MediatR.DeletePost;
using PostService.Application.MediatR.FavoritePost;
using PostService.Application.MediatR.GetFavoritePosts;
using PostService.Application.MediatR.GetLikedPosts;
using PostService.Application.MediatR.LikePost;
using PostService.Application.MediatR.Media.GetUserFeed;
using PostService.Application.MediatR.Media.GetUserPosts;
using PostService.Application.MediatR.UnfavoritePost;
using PostService.Application.MediatR.UnlikePost;
using PostService.Application.MediatR.UpdatePost;

namespace PostService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class PostController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PostController> _logger;

        public PostController(IMediator mediator, ILogger<PostController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("createpost")]
        public async Task<IActionResult> CreatePost(
            [FromBody] CreatePostRequest request)
        {
            _logger.LogInformation("🔥 HIT /createpost endpoint");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                _logger.LogWarning("❌ Unauthorized: no userId claim");
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                _logger.LogWarning("❌ Empty description");
                return BadRequest(new { error = "DESCRIPTION_REQUIRED" });
            }

            _logger.LogInformation("📦 Creating post for user {UserId}", userIdClaim.Value);

            var command = new CreatePostCommand
            {
                UserId = Guid.Parse(userIdClaim.Value),
                Description = request.Description.Trim()
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("✅ Post created successfully");

            return Ok(result);
        }
        [Authorize]
        [HttpGet("user/{userId}/cards")]
        public async Task<IActionResult> GetUserPostsProfile(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetUserPostsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [Authorize]
        [HttpGet("user/{userId}/posts")]
        public async Task<IActionResult> GetUserPostsFeed( Guid userId,[FromQuery] int page = 1,[FromQuery] int pageSize = 10)
        {
            var currentUserId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var query = new GetUserPostsFeedQuery
            {
                UserId = userId,
                CurrentUserId = currentUserId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [Authorize]
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var command = new LikePostCommand
            {
                PostId = postId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{postId}/like")]
        public async Task<IActionResult> UnlikePost(Guid postId)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var command = new UnlikePostCommand
            {
                PostId = postId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("{postId}/favorite")]
        public async Task<IActionResult> FavoritePost(Guid postId)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var command = new FavoritePostCommand
            {
                PostId = postId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{postId}/favorite")]
        public async Task<IActionResult> UnfavoritePost(Guid postId)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var command = new UnfavoritePostCommand
            {
                PostId = postId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        [Authorize]
        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavoritePosts([FromQuery] int page = 1,[FromQuery] int pageSize = 12)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var query = new GetFavoritePostsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [Authorize]
        [HttpGet("liked")]
        public async Task<IActionResult> GetLikedPosts(
         [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var query = new GetLikedPostsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [Authorize]
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(
    Guid postId,
    [FromBody] UpdatePostRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var command = new UpdatePostCommand
            {
                PostId = postId,
                UserId = userId,
                Request = request
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var command = new DeletePostCommand
            {
                PostId = postId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}