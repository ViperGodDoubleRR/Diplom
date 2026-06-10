using Microsoft.EntityFrameworkCore;

using RegService.Application.Event.User;
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
using Shared.RabbitMQ.rpc.Abstraction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddScoped<IRegRepository, EfRegRepository>();
builder.Services.AddScoped<ICodeGenerate, CodeGenerator>();

builder.Services.AddDbContext<DbContextReg>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SendEmailCommandCode).Assembly));

builder.Services.AddScoped<UserEmailUpdatedHandler>();

var app = builder.Build();

try
{
    var bus = app.Services.GetRequiredService<IEventBus>();
    var rpcServer = app.Services.GetRequiredService<IRpcServer>();

    await rpcServer.StartAsync("reg.rpc");
    bus.Subscribe<UpdateUserEmailEvent, UserEmailUpdatedHandler>();
    await bus.InitAsync();
    await bus.StartConsumingAsync("reg-service");
    app.Logger.LogInformation("RabbitMQ connected.");
}
catch (Exception ex)
{
    app.Logger.LogWarning(
        ex,
        "RabbitMQ unavailable at startup. Start RabbitMQ or check appsettings. API will run without events.");
}

app.UseCors("AllowFrontend");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DbContextReg>();
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("RegDb migrations applied.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "RegDb migration failed. Check SQL Server connection.");
    }
}

app.Run();
