using Mediator;
using Microsoft.AspNetCore.Mvc;
using LuckyMaze.Application.Command;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Queries;
using LuckyMaze.Infrastructure.Dtos;

namespace LuckyMaze.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // TODO: Add role-based authorization via custom attribute (see auth issue)
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

        [HttpPut("users/{id:guid}/credits")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdjustUserCredits(Guid id, [FromBody] AdjustCreditsDto body, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new AdjustUserCreditsCommand(id, body.Amount, body.Mode), cancellationToken);
            if (result.IsSuccess)
                return NoContent();

            return BadRequest(result.Error);
        }

        [HttpGet("all-models")]
        [ProducesResponseType(typeof(List<AvailableModelDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllModels(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllModelsQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpGet("models")]
        [ProducesResponseType(typeof(List<ModelListEntryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetModelListEntries(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetModelListEntriesQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPost("models")]
        [ProducesResponseType(typeof(ModelListEntryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddModelListEntry([FromBody] AddModelListEntryCommand command, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpDelete("models/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveModelListEntry(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new RemoveModelListEntryCommand(id), cancellationToken);
            if (result.IsSuccess)
                return NoContent();

            return BadRequest(result.Error);
        }

        [HttpGet("settings")]
        [ProducesResponseType(typeof(AdminSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminSettings(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAdminSettingsQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPut("settings")]
        [ProducesResponseType(typeof(AdminSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAdminSettings([FromBody] UpdateAdminSettingsCommand command, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }
    }
}
