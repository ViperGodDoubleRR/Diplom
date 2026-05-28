using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.MinIO.Models
{
    public class FileObject
    {
        public string Bucket { get; set; } = null!;
        public string FileKey { get; set; } = null!;

        public string ContentType { get; set; } = null!;
        public long Size { get; set; }

        public string OriginalName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
