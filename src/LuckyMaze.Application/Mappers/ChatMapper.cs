using LuckyMaze.Application.Dtos;
using LuckyMaze.Domain;

namespace LuckyMaze.Application.Mappers
{
    public static class ChatMapper
    {
        public static ChatDto ToDto(this Chat chat) =>
            new(chat.Id, chat.Title, chat.CreatedAt, chat.UpdatedAt);

        public static ChatDetailDto ToDetailDto(this Chat chat) =>
            new(chat.Id, chat.Title, chat.CreatedAt, chat.UpdatedAt, 
                chat.Messages.OrderBy(m => m.SequenceNumber).Select(m => m.ToDto()).ToList());
    }
}
