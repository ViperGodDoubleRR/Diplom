using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Application.DTO;

namespace PostService.Application.MediatR.UpdatePost
{
    public class UpdatePostCommand : IRequest<bool>
    {
        public Guid PostId { get; set; }
        public string UserId { get; set; } = null!;
        public UpdatePostRequest Request { get; set; } = null!;
    }
}
