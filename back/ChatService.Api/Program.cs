using System.Text;

using ChatService.Application.MediatR.CreatePrivateChat;
using ChatService.Application.Mapping;
using ChatService.Application.Abstractions;
using ChatService.Api.Hubs;
using ChatService.Api.Services;
using ChatService.Domain.IRepository;
using ChatService.Infrastructure.Data;
using ChatService.Infrastructure.EfRepository;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Shared.MinIO.Constants;
using Shared.MinIO.Extensions;
using Shared.MinIO.Interfaces;
using Shared.MinIO.Services;
using Shared.RabbitMQ;

const long maxUploadBytes = 310L * 1024 * 1024;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadBytes;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadBytes;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? [
                    "http://localhost:5173",
                    "http://localhost:5174",
                    "http://127.0.0.1:5173",
                    "http://127.0.0.1:5174",
                    "http://localhost:5107",
                    "http://127.0.0.1:5107"
                ])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ChatDbContext>(options =>
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                if (!path.StartsWithSegments("/hubs")) return Task.CompletedTask;

                if (context.Request.Query.TryGetValue("access_token", out var queryToken))
                {
                    var token = queryToken.ToString();
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        context.Token = token;
                        return Task.CompletedTask;
                    }
                }

                var authHeader = context.Request.Headers.Authorization.ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader["Bearer ".Length..].Trim();
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMinio(builder.Configuration);
builder.Services.AddScoped<IMinioService, MinioService>();
builder.Services.AddScoped<IChatRepository, EfChatRepository>();
builder.Services.AddScoped<IMessageRepository, EfMessageRepository>();
builder.Services.AddScoped<IChatMediaRepository, EfChatMediaRepository>();
builder.Services.AddScoped<IMessageMediaRepository, EfMessageMediaRepository>();
builder.Services.AddScoped<ChatUserResolver>();
builder.Services.AddScoped<IChatRealtimeNotifier, SignalRChatRealtimeNotifier>();

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreatePrivateChatCommand).Assembly));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ChatService.Unhandled");

        var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        if (ex is not null)
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            error = new { code = "INTERNAL", message = "Внутренняя ошибка сервера" }
        });
    });
});

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("ChatDb migrations applied.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "ChatDb migration failed. Check SQL Server connection.");
    }
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var minio = scope.ServiceProvider.GetRequiredService<IMinioService>();
        await minio.EnsureBucketAsync(Buckets.ChatImages);
        await minio.EnsureBucketAsync(Buckets.ChatVideos);
        app.Logger.LogInformation("MinIO chat buckets ready.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "MinIO unavailable at startup. Start MinIO for chat media uploads.");
    }
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
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
