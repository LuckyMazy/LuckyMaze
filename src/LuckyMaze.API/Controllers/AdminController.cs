using Mediator;
using Microsoft.AspNetCore.Mvc;
using LuckyMaze.Application.Command;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Queries;

namespace LuckyMaze.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController(IMediator mediator) : ControllerBase
    {
        [HttpGet("users")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllUsersQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPut("users/{id:guid}/lock")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetUserLock(Guid id, [FromBody] SetUserLockDto body, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new SetUserLockCommand(id, body.IsLocked), cancellationToken);
            if (result.IsSuccess)
                return NoContent();

            return BadRequest(result.Error);
        }
    }
}
