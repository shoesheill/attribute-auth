using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Attribute.Api.Authorization;
using Attribute.Api.Data;
using Attribute.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Services
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, ResourceAccessHandler>();

// Authentication & Authorization
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Jwt";
    options.DefaultChallengeScheme = "Jwt";
}).AddJwtBearer("Jwt", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? "super-secret-key-please-change-this-key-now"))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
