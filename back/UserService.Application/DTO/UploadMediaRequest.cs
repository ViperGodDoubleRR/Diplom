using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTO
{
    public class UploadMediaRequest
    {
        public IFormFile File { get; set; } = null!;

        public string MediaType { get; set; } = null!;
    }
}
