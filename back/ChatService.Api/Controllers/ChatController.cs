using ChatService.Api.Extensions;
using ChatService.Application.MediatR.ChatGroupMedia;
using ChatService.Application.MediatR.ClearChatMessages;
using ChatService.Application.MediatR.CreateGroupChat;
using ChatService.Application.MediatR.CreatePrivateChat;
using ChatService.Application.MediatR.DeleteChat;
using ChatService.Application.MediatR.GetChatById;
using ChatService.Application.MediatR.GetChatMembers;
using ChatService.Application.MediatR.GetMessages;
using ChatService.Application.MediatR.GetMyChats;
using ChatService.Application.MediatR.InviteGroupMember;
using ChatService.Application.MediatR.JoinPublicGroup;
using ChatService.Application.MediatR.RemoveGroupMember;
using ChatService.Application.MediatR.SearchPublicGroups;
using ChatService.Application.MediatR.UpdateGroupChat;
using ChatService.Application.MediatR.UploadChatAvatar;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("")]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetMyChats()
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new GetMyChatsQuery { UserId = userId.Value });
            return Ok(result);
        }

        [HttpGet("chats/{chatId:int}")]
        public async Task<IActionResult> GetChatById(int chatId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new GetChatByIdQuery
            {
                UserId = userId.Value,
                ChatId = chatId
            });

            return Ok(result);
        }

        [HttpGet("search/groups")]
        public async Task<IActionResult> SearchPublicGroups([FromQuery] string? search)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new SearchPublicGroupsQuery
            {
                UserId = userId.Value,
                Search = search ?? string.Empty
            });

            return Ok(result);
        }

        [HttpPost("chats/private")]
        public async Task<IActionResult> CreatePrivateChat([FromBody] CreatePrivateChatRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new CreatePrivateChatCommand
            {
                UserId = userId.Value,
                TargetUserId = request.TargetUserId
            });

            return Ok(result);
        }

        [HttpPost("chats/group")]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new CreateGroupChatCommand
            {
                UserId = userId.Value,
                Name = request.Name,
                IsPublic = request.IsPublic
            });

            return Ok(result);
        }

        [HttpPost("chats/{chatId:int}/members")]
        public async Task<IActionResult> InviteGroupMember(
            int chatId,
            [FromBody] InviteGroupMemberRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new InviteGroupMemberCommand
            {
                UserId = userId.Value,
                ChatId = chatId,
                TargetUserId = request.TargetUserId
            });

            return Ok(result);
        }

        [HttpDelete("chats/{chatId:int}/members/{targetUserId:guid}")]
        public async Task<IActionResult> RemoveGroupMember(int chatId, Guid targetUserId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new RemoveGroupMemberCommand
            {
                UserId = userId.Value,
                ChatId = chatId,
                TargetUserId = targetUserId
            });

            return Ok(result);
        }

        [HttpPost("chats/{chatId:int}/join")]
        public async Task<IActionResult> JoinPublicGroup(int chatId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new JoinPublicGroupCommand
            {
                UserId = userId.Value,
                ChatId = chatId
            });

            return Ok(result);
        }

        [HttpGet("chats/{chatId:int}/members")]
        public async Task<IActionResult> GetChatMembers(int chatId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new GetChatMembersQuery
            {
                UserId = userId.Value,
                ChatId = chatId
            });

            return Ok(result);
        }

        [HttpGet("chats/{chatId:int}/media")]
        public async Task<IActionResult> GetChatMedia(int chatId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new GetChatMediaQuery
            {
                UserId = userId.Value,
                ChatId = chatId
            });

            return Ok(result);
        }

        [HttpPost("chats/{chatId:int}/media")]
        [RequestSizeLimit(30L * 1024 * 1024)]
        public async Task<IActionResult> UploadChatMedia(
            int chatId,
            IFormFile file,
            [FromForm] string mediaType = "avatar")
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new UploadChatMediaCommand
            {
                UserId = userId.Value,
                ChatId = chatId,
                File = file,
                MediaType = mediaType
            });

            return Ok(result);
        }

        [HttpPut("chats/{chatId:int}/media/{mediaId:int}")]
        [RequestSizeLimit(30L * 1024 * 1024)]
        public async Task<IActionResult> ReplaceChatMedia(
            int chatId,
            int mediaId,
            IFormFile file,
            [FromForm] string mediaType = "avatar")
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new ReplaceChatMediaCommand
            {
                UserId = userId.Value,
                ChatId = chatId,
                MediaId = mediaId,
                File = file,
                MediaType = mediaType
            });

            return Ok(result);
        }

        [HttpDelete("chats/{chatId:int}/media/{mediaId:int}")]
        public async Task<IActionResult> DeleteChatMedia(int chatId, int mediaId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new DeleteChatMediaCommand
            {
                UserId = userId.Value,
                ChatId = chatId,
                MediaId = mediaId
            });

            return Ok(result);
        }

        [HttpDelete("chats/{chatId:int}/media")]
        public async Task<IActionResult> DeleteAllChatMedia(int chatId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new DeleteAllChatMediaCommand
            {
                UserId = userId.Value,
                ChatId = chatId
            });

            return Ok(result);
        }

        [HttpDelete("chats/{chatId:int}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new DeleteChatCommand
            {
                UserId = userId.Value,
                ChatId = chatId
            });

            return Ok(result);
        }

        [HttpDelete("chats/{chatId:int}/messages")]
        public async Task<IActionResult> ClearChatMessages(int chatId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new ClearChatMessagesCommand
            {
                UserId = userId.Value,
                ChatId = chatId
            });

            return Ok(result);
        }

        [HttpPatch("chats/{chatId:int}")]
        public async Task<IActionResult> UpdateGroupChat(
            int chatId,
            [FromBody] UpdateGroupChatRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new UpdateGroupChatCommand
            {
                UserId = userId.Value,
                ChatId = chatId,
                Name = request.Name,
                IsPublic = request.IsPublic
            });

            return Ok(result);
        }

        [HttpPost("chats/{chatId:int}/avatar")]
        [RequestSizeLimit(300L * 1024 * 1024)]
        public async Task<IActionResult> UploadChatAvatar(int chatId, IFormFile file)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new UploadChatAvatarCommand
            {
                UserId = userId.Value,
                ChatId = chatId,
                File = file
            });

            return Ok(result);
        }

        [HttpGet("chats/{chatId:int}/messages")]
        public async Task<IActionResult> GetMessages(
            int chatId,
            [FromQuery] int? beforeMessageId,
            [FromQuery] int limit = 50)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new GetMessagesQuery
            {
                UserId = userId.Value,
                ChatId = chatId,
                BeforeMessageId = beforeMessageId,
                Limit = limit
            });

            return Ok(result);
        }
    }

    public class CreatePrivateChatRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("targetUserId")]
        public Guid TargetUserId { get; set; }
    }

    public class CreateGroupChatRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
    }

    public class UpdateGroupChatRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool? IsPublic { get; set; }
    }

    public class InviteGroupMemberRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("targetUserId")]
        public Guid TargetUserId { get; set; }
    }
}
