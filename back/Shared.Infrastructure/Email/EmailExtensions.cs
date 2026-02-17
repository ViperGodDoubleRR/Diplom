using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Email
{
    public static class EmailExtensions
    {
        public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            var host = configuration["Smtp:Host"]
           ?? throw new InvalidOperationException("SMTP Host is missing");

            var portString = configuration["Smtp:Port"]
                             ?? throw new InvalidOperationException("SMTP Port is missing");

            var login = configuration["Smtp:Login"]
                        ?? throw new InvalidOperationException("SMTP Login is missing");

            var password = configuration["Smtp:Password"]
                           ?? throw new InvalidOperationException("SMTP Password is missing");

            var from = configuration["Smtp:From"]
                       ?? throw new InvalidOperationException("SMTP From is missing");

            services.AddScoped<IEmailSender>(_ =>
                new SmtpEmailSender(
                    host,
                    int.Parse(portString),
                    login,
                    password,
                    from
                ));
            return services;
        }
    }
}
