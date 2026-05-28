using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UserService.Application.DTO;
using UserService.Application.MediatR.AddFriend;
using UserService.Application.MediatR.BlockUser;
using UserService.Application.MediatR.GetBlocked;
using UserService.Application.MediatR.GetFriends;
using UserService.Application.MediatR.RemoveFriend;
using UserService.Application.MediatR.Social.SearchUsers;
using UserService.Application.MediatR.UnblockUser;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("")]
    public class SocialController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SocialController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // =========================
        // GET FRIENDS
        // =========================

        [HttpGet("social/friends")]
        public async Task<IActionResult> GetFriends()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();

            var command = new GetFriendsCommand
            {
                UserId = Guid.Parse(userIdClaim.Value)
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        // =========================
        // GET BLOCKED
        // =========================

        [HttpGet("social/blocked")]
        public async Task<IActionResult> GetBlocked()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();

            var command = new GetBlockedCommand
            {
                UserId = Guid.Parse(userIdClaim.Value)
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        // =========================
        // ADD FRIEND
        // =========================

        [HttpPost("social/friends")]
        public async Task<IActionResult> AddFriend(
            [FromBody] AddFriendRequest request)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();

            var command = new AddFriendCommand
            {
                MyId = Guid.Parse(userIdClaim.Value),
                FriendId = request.UserId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        // =========================
        // REMOVE FRIEND
        // =========================

        [HttpDelete("social/friends/{userId}")]
        public async Task<IActionResult> RemoveFriend(Guid userId)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();

            var command = new RemoveFriendCommand
            {
                MyId = Guid.Parse(userIdClaim.Value),
                FriendId = userId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        // =========================
        // BLOCK USER
        // =========================

        [HttpPost("social/block")]
        public async Task<IActionResult> BlockUser(
            [FromBody] BlockUserRequest request)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();

            var command = new BlockUserCommand
            {
                MyId = Guid.Parse(userIdClaim.Value),
                BlackId = request.UserId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        // =========================
        // UNBLOCK USER
        // =========================

        [HttpDelete("social/block/{userId}")]
        public async Task<IActionResult> UnblockUser(Guid userId)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();

            var command = new UnblockUserCommand
            {
                MyId = Guid.Parse(userIdClaim.Value),
                BlackId = userId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        [HttpGet("social/users")]
        public async Task<IActionResult> SearchUsers([FromQuery] SearchUsersRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                return Unauthorized();

            var query = new SearchUsersQuery
            {
                MyId = Guid.Parse(userIdClaim.Value),
                Search = request.Search,
                Page = request.Page,
                PageSize = request.PageSize
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}
