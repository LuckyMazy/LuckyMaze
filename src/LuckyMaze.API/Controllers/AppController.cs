using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Queries;

namespace LuckyMaze.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AppController(IMediator mediator) : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new AppQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpGet("Test", Name = "TestEndpointWithAuth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Test(CancellationToken cancellationToken)
        {
            return Ok();
        }
    }
}
