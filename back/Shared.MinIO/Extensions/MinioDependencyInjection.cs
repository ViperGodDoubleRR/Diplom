using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Minio;

using Shared.MinIO.Interfaces;
using Shared.MinIO.Options;
using Shared.MinIO.Services;

namespace Shared.MinIO.Extensions
{
    public static class MinioDependencyInjection
    {
        public static IServiceCollection AddMinio(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.Configure<MinioOptions>(config.GetSection("Minio"));

            var options = new MinioOptions();
            config.GetSection("Minio").Bind(options);

            var client = new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(
                    options.AccessKey,
                    options.SecretKey
                )
                .WithSSL(options.Secure)
                .Build();

            services.AddSingleton<IMinioClient>(
                client
            );

            services.AddScoped<IMinioService,
                MinioService>();

            return services;
        }
    }
}