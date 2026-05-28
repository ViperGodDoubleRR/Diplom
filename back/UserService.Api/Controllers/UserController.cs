using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Shared.Application.Contracts;
using Shared.RabbitMQ.rpc.Abstraction;

using UserService.Application.DTO;
using UserService.Application.MediatR.Profile.GetUserById;
using UserService.Application.MediatR.UpdateUser;
using UserService.Application.MediatR.UserCommand;
using UserService.Domain.Models;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;
        public UserController(IMediator mediator, ILogger<UserController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [Authorize]
        [HttpGet("get-user")]
        public async Task<IActionResult> GetUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                return Unauthorized();
            }

            Guid userId = Guid.Parse(userIdClaim.Value);
            var command = new GetUserCommand
            {
                UserId = userId
            };

            ApiResponse<User> response = await _mediator.Send(command);

            return Ok(response);
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                return Unauthorized();
            }

            Guid userId = Guid.Parse(userIdClaim.Value);

            var command = new UpdateUserCommand
            {
                UserId = userId,
                Dto = dto
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var query = new GetUserByIdQuery
            {
                UserId = id
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}
