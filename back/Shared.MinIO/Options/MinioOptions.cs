using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.MinIO.Options
{
    public class MinioOptions
    {
        public string Endpoint { get; set; } = null!;
        public string AccessKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public bool Secure { get; set; }
    }
}
