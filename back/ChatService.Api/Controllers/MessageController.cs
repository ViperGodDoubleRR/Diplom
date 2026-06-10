using ChatService.Api.Extensions;
using ChatService.Application.MediatR.DeleteMessage;
using ChatService.Application.MediatR.DeleteMessageMedia;
using ChatService.Application.MediatR.SendMessage;
using ChatService.Application.MediatR.UpdateMessage;
using ChatService.Application.MediatR.UploadMessageMedia;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("")]
    public class MessageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MessageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("chats/{chatId:int}/messages")]
        public async Task<IActionResult> SendMessage(
            int chatId,
            [FromBody] SendMessageRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new SendMessageCommand
            {
                UserId = userId.Value,
                ChatId = chatId,
                Text = request.Text,
                ReplyToMessageId = request.ReplyToMessageId
            });

            return Ok(result);
        }

        [HttpPut("messages/{messageId:int}")]
        public async Task<IActionResult> UpdateMessage(
            int messageId,
            [FromBody] UpdateMessageRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new UpdateMessageCommand
            {
                UserId = userId.Value,
                MessageId = messageId,
                Text = request.Text
            });

            return Ok(result);
        }

        [HttpDelete("messages/{messageId:int}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new DeleteMessageCommand
            {
                UserId = userId.Value,
                MessageId = messageId
            });

            return Ok(result);
        }

        [HttpDelete("messages/{messageId:int}/media/{mediaId:int}")]
        public async Task<IActionResult> DeleteMessageMedia(int messageId, int mediaId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new DeleteMessageMediaCommand
            {
                UserId = userId.Value,
                MessageId = messageId,
                MediaId = mediaId
            });

            return Ok(result);
        }

        [HttpPost("messages/{messageId:int}/media")]
        [RequestSizeLimit(300L * 1024 * 1024)]
        public async Task<IActionResult> UploadMessageMedia(
            int messageId,
            IFormFile file,
            [FromForm] string mediaType = "image")
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var result = await _mediator.Send(new UploadMessageMediaCommand
            {
                UserId = userId.Value,
                MessageId = messageId,
                File = file,
                MediaType = mediaType
            });

            return Ok(result);
        }
    }

    public class SendMessageRequest
    {
        public string Text { get; set; } = string.Empty;
        public int? ReplyToMessageId { get; set; }
    }

    public class UpdateMessageRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}
