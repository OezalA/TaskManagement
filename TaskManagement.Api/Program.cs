using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using TaskManagement.Api.Middleware;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// for MCP
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiAccess", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.Claims.Any(c =>
                (c.Type == "scp" ||
                 c.Type == "http://schemas.microsoft.com/identity/claims/scope")
                && c.Value.Contains("access_api")));
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();

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
    // Swashbuckle v10 uyumlu SecurityRequirement
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(schemeId, document)]
            = new List<string> { scope }
    });
});


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
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


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
