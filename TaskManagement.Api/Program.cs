using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using TaskManagement.Api.Middleware;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Services;
using TaskManagement.MCP;

var builder = WebApplication.CreateBuilder(args);

// Entra ID (Azure AD) bearer-token validation for the API.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiAccess", policy =>
        policy.RequireAuthenticatedUser());

    // Every endpoint requires an authenticated user unless it opts out with [AllowAnonymous].
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IWorkLogService, WorkLogService>();
builder.Services.AddScoped<IWorkLogQueryService, WorkLogQueryService>();
builder.Services.AddScoped<IUserResolutionService, UserResolutionService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<MCPToolHandler>((provider, client) =>
{
    var apiBaseUrl = builder.Configuration["MCPSettings:ApiBaseUrl"] ?? "http://localhost:5253";
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var apiBaseUrl = builder.Configuration["MCPSettings:ApiBaseUrl"] ?? "http://localhost:5253";
    return new MCPToolHandler(httpClient, apiBaseUrl);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    const string schemeId = "oauth2";

    var tenantId =
        builder.Configuration["SwaggerAzureAd:TenantId"]
        ?? throw new InvalidOperationException("Missing config: SwaggerAzureAd:TenantId");

    var clientId =
        builder.Configuration["AzureAd:ClientId"]
        ?? throw new InvalidOperationException("Missing config: AzureAd:ClientId");

    var scope = $"api://{clientId}/access_api";

    options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(
                    $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"
                ),
                TokenUrl = new Uri(
                    $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"
                ),
                Scopes = new Dictionary<string, string>
                {
                    [scope] = "Access TaskManagement API"
                }
            }
        }
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(schemeId, document)]
            = new List<string> { scope }
    });
});


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(
            builder.Configuration["SwaggerAzureAd:ClientId"]
        );
        options.OAuthUsePkce();
        options.OAuthScopeSeparator(" ");
    });
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
