using Microsoft.EntityFrameworkCore;

using RegService.Application.Events;
using RegService.Application.Mediatr.SendEmail;
using RegService.Domain.IRepository;
using RegService.Infrastructure.Data;
using RegService.Infrastructure.IEfRepository;

using Shared.Application.Interfaces;
using Shared.Infrastructure.Email;
using Shared.Infrastructure.Security;
using Shared.RabbitMQ;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;
using Shared.RabbitMQ.EventBus.RabbitMQ;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//rabbit and handler
builder.Services.AddRabbitMq(builder.Configuration);

//shared
builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddScoped<IRegRepository, EfRegRepository>();
builder.Services.AddScoped<IHasher,Hasher>();
builder.Services.AddScoped<ICodeGenerate, CodeGenerator>();
builder.Services.AddDbContext<DbContextReg>(options =>
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

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContextReg>();
    db.Database.Migrate();
}

var bus = app.Services.GetRequiredService<IEventBus>();
await bus.InitAsync();
app.UseCors("AllowAll");
    
//app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "reg" }));

app.MapControllers();

app.Run();
