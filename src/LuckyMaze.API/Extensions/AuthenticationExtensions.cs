using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using LuckyMaze.Infrastructure.Services;
using System.Security.Claims;

namespace LuckyMaze.API.Extensions
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddUserSync(this AuthenticationBuilder builder)
        {
            builder.Services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Events ??= new JwtBearerEvents();
                    var previous = options.Events.OnTokenValidated;

                    options.Events.OnTokenValidated = async context =>
                    {
                        if (previous is not null)
                            await previous(context);

                        if (context.Principal?.Identity is not ClaimsIdentity identity)
                            return;

                        context.HttpContext.User = context.Principal;

                        var externalId = identity.FindFirst("sub")?.Value ?? identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        if (string.IsNullOrEmpty(externalId))
                            return;

                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<LuckyMaze.Infrastructure.LuckyMazeDbContext>();

                        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(
                            dbContext.Users, u => u.ExternalId == externalId, context.HttpContext.RequestAborted);

                        if (user is null)
                        {
                            await userService.SyncCurrentUserAsync(context.HttpContext.RequestAborted);
                            user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(
                                dbContext.Users, u => u.ExternalId == externalId, context.HttpContext.RequestAborted);
                        }

                        if (user is not null && user.Role == LuckyMaze.Domain.Enums.UserRole.Admin)
                        {
                            identity.AddClaim(new Claim("groups", "admin"));
                        }
                    };
                });

            return builder;
        }
    }
}
