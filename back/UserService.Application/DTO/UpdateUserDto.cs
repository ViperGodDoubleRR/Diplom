using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace UserService.Application.DTO
    {
        public class UpdateUserDto
        {
            public string Login { get; set; } = string.Empty;

            public string? Tag { get; set; }
            public string? Description { get; set; }
        }
    }