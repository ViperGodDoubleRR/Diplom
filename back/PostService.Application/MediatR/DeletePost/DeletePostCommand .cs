using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace PostService.Application.MediatR.DeletePost
{
    public class DeletePostCommand : IRequest<bool>
    {
        public Guid PostId { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
