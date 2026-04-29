using Mediator;
using Microsoft.AspNetCore.Mvc;
using LuckyMaze.Application.Command;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Queries;

namespace LuckyMaze.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(List<ChatDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChats(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetChatsQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ChatDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetChat(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetChatQuery(id), cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SendMessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand command, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RenameChat(Guid id, [FromBody] RenameChatCommand command, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command with { ChatId = id }, cancellationToken);
            if (result.IsSuccess)
                return NoContent();

            return BadRequest(result.Error);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteChat(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteChatCommand(id), cancellationToken);
            if (result.IsSuccess)
                return NoContent();

            return BadRequest(result.Error);
        }
    }
}
