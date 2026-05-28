using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegService.Domain.Models
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public bool IsProcessed { get; set; }
    }
}
