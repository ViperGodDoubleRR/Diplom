using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentService.Domain.Models
{

    public class CommentMedia
    {
        public int Id { get; set; }

        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        public string Bucket { get; set; } = null!;
        public string FileKey { get; set; } = null!;
        public string OriginalName { get; set; } = null!;
        public string MediaType { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Size { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
