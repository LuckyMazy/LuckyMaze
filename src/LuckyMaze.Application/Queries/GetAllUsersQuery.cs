using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Mappers;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Queries
{
    public record GetAllUsersQuery() : IQuery<Result<List<UserDto>>>;

    public class GetAllUsersQueryHandler(LuckyMazeDbContext dbContext) : IQueryHandler<GetAllUsersQuery, Result<List<UserDto>>>
    {
        public async ValueTask<Result<List<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
        {
            var users = await dbContext.Users
                .OrderBy(u => u.DisplayName)
                .ToListAsync(cancellationToken);

            return Result<List<UserDto>>.Success(users.Select(u => u.ToDto()).ToList());
        }
    }
}
