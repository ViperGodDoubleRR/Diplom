using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Application.DTO;
using Microsoft.AspNetCore.Http;
namespace UserService.Application.MediatR.UploadAvatar
{
    public class UploadAvatarCommand
         : IRequest<ApiResponse<MediaDto>>
    {
        public Guid UserId { get; set; }

        public IFormFile File { get; set; } = null!;

        public string MediaType { get; set; } = null!;
    }
}
