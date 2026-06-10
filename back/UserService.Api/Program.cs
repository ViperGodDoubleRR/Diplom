using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Shared.MinIO.Constants;
using Shared.MinIO.Extensions;
using Shared.MinIO.Interfaces;
using Shared.MinIO.Services;
using Shared.RabbitMQ;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;
using Shared.RabbitMQ.EventBus.RabbitMQ;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetUserPost;

using UserService.Application.Event.UserCreate;
using UserService.Application.Event.UserEmail;
using UserService.Application.MediatR.UserCommand;
using UserService.Application.RPC;
using UserService.Domain.IRepository;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.EfRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddDbContext<DbContextUser>(options =>
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMinio(builder.Configuration);
builder.Services.AddScoped<IMinioService, MinioService>();

builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<ISocialRepository, EfSocialRepository>();
builder.Services.AddScoped<IMediaRepository, EfMediaRepository>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetUserCommand).Assembly));

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddScoped<UserRegisteredHandler>();
builder.Services.AddScoped<UserEmailUpdatedHandler>();
builder.Services.AddScoped<IRPCHandle<GetUserRpcRequest, GetUserRpcResponse>, GetUserRpcHandler>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

try
{
    var rpcServer = app.Services.GetRequiredService<IRpcServer>();
    var bus = app.Services.GetRequiredService<IEventBus>();

    await rpcServer.StartAsync("user.rpc");
    bus.Subscribe<CreateUserEvent, UserRegisteredHandler>();
    bus.Subscribe<UpdateUserEmailEvent, UserEmailUpdatedHandler>();
    await bus.InitAsync();
    await bus.StartConsumingAsync("user-service");

    app.Logger.LogInformation("RabbitMQ connected.");
}
catch (Exception ex)
{
    app.Logger.LogWarning(
        ex,
        "RabbitMQ unavailable at startup. Start RabbitMQ or check appsettings. API will run without events/RPC.");
}

try
{
    using var scope = app.Services.CreateScope();
    var minio = scope.ServiceProvider.GetRequiredService<IMinioService>();
    await minio.EnsureBucketAsync(Buckets.UserAvatars);
    await minio.EnsureBucketAsync(Buckets.UserGallery);
    app.Logger.LogInformation("MinIO buckets ready.");
}
catch (Exception ex)
{
    app.Logger.LogWarning(
        ex,
        "MinIO unavailable at startup. Start MinIO on localhost:9000 for media uploads.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DbContextUser>();
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("UserDb migrations applied.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "UserDb migration failed. Check SQL Server connection.");
    }
}

app.Run();
