using Mediator;
using Microsoft.AspNetCore.Mvc;
using LuckyMaze.Application.Queries;
using LuckyMaze.Infrastructure.Dtos;

namespace LuckyMaze.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelController(IMediator mediator) : ControllerBase
    {
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<AvailableModelDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableModels(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAvailableModelsQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }
    }
}
