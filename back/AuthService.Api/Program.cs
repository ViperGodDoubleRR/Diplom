using AuthService.Application.MediatR.ResCheckCode;
using AuthService.Application.Settings;
using AuthService.Domain.Interface;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.EfRepository;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Shared.Application.Interfaces;
using Shared.Infrastructure.Email;
using Shared.Infrastructure.JWT;
using Shared.Infrastructure.Security;
using Shared.RabbitMQ;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.rpc.Abstraction;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<DbContextAuth>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddScoped<IHasher, Hasher>();
builder.Services.AddScoped<ICodeGenerate, CodeGenerator>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IAuthRepository, EfAuthRepository>();
builder.Services.AddScoped<IResRepository, EfResRepository>();
builder.Services.AddScoped<EmailChangeAvailabilityChecker>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ResCheckCodeCommand).Assembly));

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

try
{
    var bus = app.Services.GetRequiredService<IEventBus>();
    await bus.InitAsync();

    var rpcServer = app.Services.GetRequiredService<IRpcServer>();
    await rpcServer.StartAsync("auth.rpc");
    app.Logger.LogInformation("RabbitMQ RPC server started.");
}
catch (Exception ex)
{
    app.Logger.LogWarning(
        ex,
        "RabbitMQ unavailable at startup. Start RabbitMQ or check appsettings. Auth RPC disabled.");
}

app.UseForwardedHeaders();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DbContextAuth>();
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("AuthDb migrations applied.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "AuthDb migration failed. Check SQL Server connection.");
    }
}

app.Run();
