using LuckyMazy.Application.Dtos;
using LuckyMazy.Application.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LuckyMaze.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppController(IMediator mediator) : ControllerBase
    {
        [HttpGet("", Name = "GetAppInfo")]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<AppDto>> Get()
        {
            var result = await mediator.Send(new AppQuery());
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }
    }
}