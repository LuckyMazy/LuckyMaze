using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Mappers;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Services;

namespace LuckyMaze.Application.Queries
{
    public record GetCurrentUserQuery() : IQuery<Result<UserDto>>;

    public class GetCurrentUserQueryHandler(IUserService userService, LuckyMazeDbContext dbContext) : IQueryHandler<GetCurrentUserQuery, Result<UserDto>>
    {
        public async ValueTask<Result<UserDto>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
        {
            var userId = await userService.GetCurrentUserIdAsync(cancellationToken);
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return Result<UserDto>.Failure("User not found");

            return Result<UserDto>.Success(user.ToDto());
        }
    }
}
