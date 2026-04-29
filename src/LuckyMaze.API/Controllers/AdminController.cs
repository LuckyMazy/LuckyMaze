using Mediator;
using Microsoft.AspNetCore.Mvc;
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
    }
}
