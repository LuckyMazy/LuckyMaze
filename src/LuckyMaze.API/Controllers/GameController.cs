using Mediator;
using Microsoft.AspNetCore.Mvc;
using LuckyMaze.Application.Command;
using LuckyMaze.Application.Queries;
using LuckyMaze.Application.Dtos;

namespace LuckyMaze.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController(IMediator mediator) : ControllerBase
    {
        [HttpGet("current")]
        [ProducesResponseType(typeof(ActiveGameDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCurrentGame(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetActiveGameQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(List<GameHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGameHistory([FromQuery] int limit = 20, CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetGameHistoryQuery(limit), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPost("ready")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ToggleReady([FromBody] ToggleReadyRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ToggleReadyCommand(request.IsReady), cancellationToken);
            if (result.IsSuccess)
                return NoContent();

            return BadRequest(result.Error);
        }

        [HttpPost("bet")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new PlaceBetCommand(request.ExitName, request.Amount), cancellationToken);
            if (result.IsSuccess)
                return NoContent();

            return BadRequest(result.Error);
        }
    }

    #region Request models

    public class ToggleReadyRequest
    {
        public bool IsReady { get; set; }
    }

    public class PlaceBetRequest
    {
        public required string ExitName { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion
}
