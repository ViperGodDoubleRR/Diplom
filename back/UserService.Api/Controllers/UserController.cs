
using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Application.Contracts;

using UserService.Application.DTO;
using UserService.Application.MediatR.Profile.GetUserById;
using UserService.Application.MediatR.UpdateUser;
using UserService.Application.MediatR.UserCommand;
using UserService.Api.Extensions;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("get-user")]
        public async Task<IActionResult> GetUser()
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var response = await _mediator.Send(new GetUserCommand { UserId = userId.Value });
            return Ok(response);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var response = await _mediator.Send(new UpdateUserCommand
            {
                UserId = userId.Value,
                Dto = dto
            });

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var response = await _mediator.Send(new GetUserByIdQuery { UserId = id });
            return Ok(response);
        }
    }
}
