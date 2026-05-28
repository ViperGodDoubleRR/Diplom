using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTO
{
    public class ReplaceMediaRequest
    {
        public IFormFile File { get; set; }
        public string MediaType { get; set; }
    }
}
