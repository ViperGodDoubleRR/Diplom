using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PostService.Api.Extensions;
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
    [Authorize]
    [Route("")]
    public class PostController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PostController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("createpost")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new CreatePostCommand
            {
                UserId = userId.Value,
                Description = request.Description
            });

            return Ok(result);
        }

        [HttpGet("user/{userId:guid}/cards")]
        public async Task<IActionResult> GetUserPostsProfile(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetUserPostsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("user/{userId:guid}/posts")]
        public async Task<IActionResult> GetUserPostsFeed(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var currentUserId = User.GetUserId();
            if (currentUserId is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetUserPostsFeedQuery
            {
                UserId = userId,
                CurrentUserId = currentUserId.Value,
                Page = page,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpPost("{postId:guid}/like")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new LikePostCommand
            {
                PostId = postId,
                UserId = userId.Value
            });

            return Ok(result);
        }

        [HttpDelete("{postId:guid}/like")]
        public async Task<IActionResult> UnlikePost(Guid postId)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new UnlikePostCommand
            {
                PostId = postId,
                UserId = userId.Value
            });

            return Ok(result);
        }

        [HttpPost("{postId:guid}/favorite")]
        public async Task<IActionResult> FavoritePost(Guid postId)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new FavoritePostCommand
            {
                PostId = postId,
                UserId = userId.Value
            });

            return Ok(result);
        }

        [HttpDelete("{postId:guid}/favorite")]
        public async Task<IActionResult> UnfavoritePost(Guid postId)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new UnfavoritePostCommand
            {
                PostId = postId,
                UserId = userId.Value
            });

            return Ok(result);
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavoritePosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetFavoritePostsQuery
            {
                UserId = userId.Value,
                Page = page,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("liked")]
        public async Task<IActionResult> GetLikedPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetLikedPostsQuery
            {
                UserId = userId.Value,
                Page = page,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpPut("{postId:guid}")]
        public async Task<IActionResult> UpdatePost(
            Guid postId,
            [FromBody] UpdatePostRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new UpdatePostCommand
            {
                PostId = postId,
                UserId = userId.Value,
                Request = request
            });

            return Ok(result);
        }

        [HttpDelete("{postId:guid}")]
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new DeletePostCommand
            {
                PostId = postId,
                UserId = userId.Value
            });

            return Ok(result);
        }
    }
}
