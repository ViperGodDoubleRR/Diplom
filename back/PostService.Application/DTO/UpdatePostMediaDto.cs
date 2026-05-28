using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace PostService.Application.DTO
{
    public class UpdatePostMediaDto
    {
        public Guid? Id { get; set; }      
        public string? Url { get; set; }     
        public IFormFile? File { get; set; } 
        public bool IsDeleted { get; set; }
    }
}
