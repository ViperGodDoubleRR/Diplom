using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PostService.Infrastructure.Data;
using Shared.MinIO.Interfaces;
using Shared.MinIO.Services;
using Shared.RabbitMQ;
using PostService.Domain.IRepository;
using PostService.Infrastructure.EfRepository;
using PostService.Application.MediatR.CreatePost;
using Shared.MinIO.Extensions;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitMq();
builder.Services.AddScoped<IPostRepository, EfPostRepository>();
builder.Services.AddScoped<IPostMediaRepository,EfPostMediaRepository>();

builder.Services.AddControllers();

builder.Services.AddMinio(builder.Configuration);
builder.Services.AddScoped<IMinioService, MinioService>();
    
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreatePostCommand).Assembly));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DbContextPost>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
