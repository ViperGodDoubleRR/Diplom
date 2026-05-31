
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Shared.Application.Interfaces;
using Shared.Infrastructure.Email;
using Shared.Infrastructure.Security;
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
using UserService.Application.MediatR.SendEmail;
using UserService.Application.RPC;
using UserService.Domain.IRepository;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.EfRepository;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddScoped<UserRegisteredHandler>();
builder.Services.AddScoped<IRPCHandle<GetUserRpcRequest, GetUserRpcResponse>,GetUserRpcHandler>();
builder.Services.AddControllers();

builder.Services.AddMinio(builder.Configuration);
builder.Services.AddScoped<IMinioService, MinioService>();



builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<ISocialRepository, EfSocialRepository>();
builder.Services.AddScoped<IMediaRepository, EfMediaRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//shared
builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddScoped<IHasher, Hasher>();
builder.Services.AddScoped<ICodeGenerate, CodeGenerator>();
builder.Services.AddDbContext<DbContextUser>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SendEmailCommandCode).Assembly));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContextUser>();
    db.Database.Migrate();
}

var bus = app.Services.GetRequiredService<IEventBus>();
var rpcServer = app.Services.GetRequiredService<IRpcServer>();
rpcServer.Start("user.rpc");
bus.Subscribe<CreateUserEvent, UserRegisteredHandler>();

await bus.InitAsync();
await bus.StartConsumingAsync("user-service");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "user" }));
app.MapControllers();

app.Run();
