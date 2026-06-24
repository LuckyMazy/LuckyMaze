using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using LuckyMaze.API.Extensions;
using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Services;
using LuckyMaze.Application.Services;
using LuckyMaze.API.Services;
using LuckyMaze.API.Hubs;
using LuckyMaze.Domain;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSwaggerGen(options =>
{
    var authority = builder.Configuration["Oidc:Authority"]
        ?? throw new InvalidOperationException("Oidc:Authority not configured");

    options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{authority}/authorize"),
                TokenUrl = new Uri($"{authority}/api/oidc/token"),
                Scopes = new Dictionary<string, string>
                {
                    ["openid"] = "OpenID Connect",
                    ["profile"] = "User profile",
                    ["email"] = "User email",
                    ["groups"] = "User groups (roles)",
                    ["picture"] = "Profile Picture",
                }
            }
        }
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("OAuth2", document)] = new List<string>()
    });
});

builder.Services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

builder.Services.AddDbContext<LuckyMazeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("LuckyMazeDatabase"),
        npgsqlOptions => npgsqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null
            )
    ));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddScoped<IOidcService, OidcService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameSettingsService, GameSettingsService>();
builder.Services.AddSingleton<IMazeGenerator, MazeGenerator>();
builder.Services.AddSingleton<IAiSolver, AiSolver>();
builder.Services.AddSingleton<IMazeHardwareService, MazeHardwareService>();
builder.Services.AddSingleton<IGameNotificationService, GameNotificationService>();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<GameManager>());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Oidc:Authority"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Oidc:RequireHttpsMetadata", true);
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "groups";
        options.TokenValidationParameters.ValidateAudience = false;

        // SignalR transports can't send an Authorization header, so the token
        // arrives as an access_token query parameter on hub requests.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddUserSync();

builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();

app.ApplyMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(builder.Configuration["Oidc:ClientId"]);
        options.OAuthUsePkce();
        options.OAuthScopes("openid", "profile", "email", "groups", "picture");

        options.UseRequestInterceptor(
            "(req) => { if (req.url.includes('/oidc/token')) { delete req.headers['X-Requested-With']; } return req; }"
        );
    });
}

app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/hubs/game");

app.Run();
