using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.MinIO.Models
{
    public class UploadFileResult
    {
        public string FileKey { get; set; } = null!;
        public string Bucket { get; set; } = null!;
        public string Url { get; set; } = null!;

        public string ContentType { get; set; } = null!;
        public long Size { get; set; }

        public string OriginalName { get; set; } = null!;
    }
}
