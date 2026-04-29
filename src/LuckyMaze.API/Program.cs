using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using LuckyMaze.API.Extensions;
using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Clients;
using LuckyMaze.Infrastructure.Extensions;
using LuckyMaze.Infrastructure.Services;
using LuckyMaze.Infrastructure.BackgroundServices;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger / OpenAPI
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

// Mediator & Validation
builder.Services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

// DBContext
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

builder.Services.AddHttpClient();
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// Services
builder.Services.AddScoped<IOidcService, OidcService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOpenRouterClient, OpenRouterClient>();
builder.Services.AddScoped<ICreditsService, CreditsService>();
builder.Services.AddHostedServices();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Oidc:Authority"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Oidc:RequireHttpsMetadata", true);
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "groups";
        options.TokenValidationParameters.ValidateAudience = false;
    })
    .AddUserSync();

// Authorization
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();

// DB
app.ApplyMigrations();
await app.ApplySeedsAsync();

// Configure the HTTP request pipeline.
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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();