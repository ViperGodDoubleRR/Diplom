using AuthService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

using Shared.Application.Interfaces;
using Shared.Infrastructure.Email;
using Shared.Infrastructure.Security;
using Shared.RabbitMQ;
using Shared.RabbitMQ.rpc.Abstraction;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitMq();
//shared
builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddScoped<IHasher, Hasher>();
builder.Services.AddScoped<ICodeGenerate, CodeGenerator>();
builder.Services.AddDbContext<DbContextAuth>(options =>
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



var app = builder.Build();
var server = app.Services.GetRequiredService<IRpcServer>();
server.Start("auth.rpc");

app.MapControllers();
app.Run();
