using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace PostService.Application.DTO
{
    public class UpdatePostRequest
    {
        public string Description { get; set; } = string.Empty;

        public List<PostMediaDto> Media { get; set; } = new();
    }
}
