using LuckyMaze.Application.Dtos;
using LuckyMaze.Domain;
using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Application.Mappers
{
    public static class UserMapper
    {
        public static UserDto ToDto(this User user) =>
            new(user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.Role, user.CreditBalance, user.IsLocked);
    }
}
