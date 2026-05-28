using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Application.DTO;

namespace UserService.Application.MediatR.Social.SearchUsers
{
    public class SearchUsersQuery : IRequest<ApiResponse<List<UserPreviewDto>>>
    {
        public Guid MyId { get; set; }
        public string Search { get; set; } = "";
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
