using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.DTO
{
    public class SearchUsersRequest
    {
        public string? Search { get; set; } = "";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
