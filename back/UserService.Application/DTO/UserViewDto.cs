using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.DTO
{
    public class UserViewDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Tag { get; set; }
        public string? Description { get; set; }
        public List<MediaDto>? Media { get; set; }
    }
}
