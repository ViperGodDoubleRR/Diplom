using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.DTO
{
    public class MediaDto
    {
        public int Id { get; set; }
        public string FileKey { get; set; } = null!;
        public string Bucket { get; set; } = null!;
        public string MediaType { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string Url { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
