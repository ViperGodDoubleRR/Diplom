using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using PostService.Application.DTO;
using PostService.Application.MediatR.Media.UploadPostMedia;

namespace PostService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class PostMediaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PostMediaController> _logger;

        public PostMediaController(IMediator mediator, ILogger<PostMediaController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("{postId}/media")]
        public async Task<IActionResult> UploadPostMedia(
            Guid postId,
            [FromForm] UploadPostMediaRequest request)
        {
            _logger.LogInformation("🔥 HIT /{PostId}/media", postId);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                _logger.LogWarning("❌ Unauthorized upload media");
                return Unauthorized();
            }

            _logger.LogInformation("📤 Upload media for post {PostId} by user {UserId}",
                postId, userIdClaim.Value);

            var command = new UploadPostMediaCommand
            {
                UserId = Guid.Parse(userIdClaim.Value),
                PostId = postId,
                File = request.File,
                MediaType = request.MediaType
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("✅ Media uploaded for post {PostId}", postId);

            return Ok(result);
        }
    }
}