using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Email
{
    public static class EmailExtensions
    {
        public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            var host = configuration["Smtp:Host"];

            var portString = configuration["Smtp:Port"];

            var login = configuration["Smtp:Login"];

            var password = configuration["Smtp:Password"];

            var from = configuration["Smtp:From"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portString) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(from) ||
                password.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<IEmailSender, ConsoleEmailSender>();
                return services;
            }

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
