using System.Text;

using CommentService.Application.MediatR.CreateComment;
using CommentService.Application.RPC;
using CommentService.Domain.IRepository;
using CommentService.Infrastructure.Data;
using CommentService.Infrastructure.EfRepository;

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
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetPostCommentsCounts;

const long maxUploadBytes = 35L * 1024 * 1024;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? ["http://localhost:5173", "http://localhost:5174"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<CommentDbContext>(options =>
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
builder.Services.AddScoped<ICommentRepository, EfCommentRepository>();
builder.Services.AddScoped<ICommentMediaRepository, EfCommentMediaRepository>();
builder.Services.AddScoped<
    IRPCHandle<GetPostCommentsCountsRpcRequest, GetPostCommentsCountsRpcResponse>,
    GetPostCommentsCountsRpcHandler>();

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateCommentCommand).Assembly));

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
    await rpcServer.StartAsync("comment.rpc");
    app.Logger.LogInformation("Comment RPC server started.");
}
catch (Exception ex)
{
    app.Logger.LogWarning(
        ex,
        "RabbitMQ unavailable at startup. Start RabbitMQ for comment RPC.");
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var minio = scope.ServiceProvider.GetRequiredService<IMinioService>();
        await minio.EnsureBucketAsync(Buckets.CommentImages);
        await minio.EnsureBucketAsync(Buckets.CommentVideos);
        app.Logger.LogInformation("MinIO comment buckets ready.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "MinIO unavailable at startup. Start MinIO on localhost:9000 for comment media uploads.");
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

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CommentDbContext>();
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("CommentDb migrations applied.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "CommentDb migration failed. Check SQL Server connection.");
    }
}

app.Run();
