using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Http;

using PostService.Application.DTO;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.Media.UploadPostMedia
{
    public class UploadPostMediaCommand
        : IRequest<ApiResponse<PostMediaDto>>
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }

        public IFormFile File { get; set; } = null!;
        public string MediaType { get; set; } = null!;
    }
}
