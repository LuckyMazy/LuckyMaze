using LuckyMaze.Application.Dtos;
using LuckyMaze.Domain;

namespace LuckyMaze.Application.Mappers
{
    public static class MessageMapper
    {
        public static MessageDto ToDto(this Message message) =>
            new(message.Id, message.Role, message.Content, message.Model, message.ToolCalls, message.ToolCallId, message.FinishReason, message.SequenceNumber, message.CreatedAt);
    }
}
