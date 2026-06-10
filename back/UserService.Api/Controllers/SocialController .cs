
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
using UserService.Application.MediatR.RenameFriend;
using UserService.Application.MediatR.UnblockUser;
using UserService.Api.Extensions;

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

        [HttpGet("social/friends")]
        public async Task<IActionResult> GetFriends()
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetFriendsCommand { UserId = userId.Value });
            return Ok(result);
        }

        [HttpGet("social/blocked")]
        public async Task<IActionResult> GetBlocked()
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetBlockedCommand { UserId = userId.Value });
            return Ok(result);
        }

        [HttpPost("social/friends")]
        public async Task<IActionResult> AddFriend([FromBody] AddFriendRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new AddFriendCommand
            {
                MyId = userId.Value,
                FriendId = request.UserId
            });

            return Ok(result);
        }

        [HttpDelete("social/friends/{userId:guid}")]
        public async Task<IActionResult> RemoveFriend(Guid userId)
        {
            var myId = User.GetUserId();
            if (myId is null)
                return Unauthorized();

            var result = await _mediator.Send(new RemoveFriendCommand
            {
                MyId = myId.Value,
                FriendId = userId
            });

            return Ok(result);
        }

        [HttpPost("social/block")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new BlockUserCommand
            {
                MyId = userId.Value,
                BlackId = request.UserId
            });

            return Ok(result);
        }

        [HttpDelete("social/block/{userId:guid}")]
        public async Task<IActionResult> UnblockUser(Guid userId)
        {
            var myId = User.GetUserId();
            if (myId is null)
                return Unauthorized();

            var result = await _mediator.Send(new UnblockUserCommand
            {
                MyId = myId.Value,
                BlackId = userId
            });

            return Ok(result);
        }

        [HttpPatch("social/friends/{userId:guid}/rename")]
        public async Task<IActionResult> RenameFriend(
            Guid userId,
            [FromBody] RenameFriendRequest request)
        {
            var myId = User.GetUserId();
            if (myId is null)
                return Unauthorized();

            var result = await _mediator.Send(new RenameFriendCommand
            {
                MyId = myId.Value,
                FriendId = userId,
                Login = request.Login
            });

            return Ok(result);
        }

        [HttpGet("social/users")]
        public async Task<IActionResult> SearchUsers([FromQuery] SearchUsersRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _mediator.Send(new SearchUsersQuery
            {
                MyId = userId.Value,
                Search = request.Search,
                Page = request.Page,
                PageSize = request.PageSize
            });

            return Ok(result);
        }
    }
}
