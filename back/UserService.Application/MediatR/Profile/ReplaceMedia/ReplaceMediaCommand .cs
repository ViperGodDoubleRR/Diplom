using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Http;

using Shared.Application.Contracts;

using UserService.Application.DTO;

namespace UserService.Application.MediatR.ReplaceMedia
{
    public class ReplaceMediaCommand : IRequest<ApiResponse<MediaDto>>
    {
        public Guid UserId { get; set; }
        public int MediaId { get; set; }
        public IFormFile File { get; set; }
        public string MediaType { get; set; }
    }
}
