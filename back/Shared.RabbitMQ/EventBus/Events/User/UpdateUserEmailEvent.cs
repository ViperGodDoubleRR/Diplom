using System;

using Shared.RabbitMQ.EventBus.Abstractions;

namespace Shared.RabbitMQ.EventBus.Events.User
{
    public class UpdateUserEmailEvent : IIntegrationEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string OldEmail { get; set; } = string.Empty;
    }
}
