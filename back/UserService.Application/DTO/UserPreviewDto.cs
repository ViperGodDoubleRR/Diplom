using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.DTO
{
    public class UserPreviewDto
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = null!;

        public string? Tag { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
