using MediatR;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.DeletePost
{
    public class DeletePostCommand : IRequest<ApiResponse<bool>>
    {
        public Guid PostId { get; set; }

        public Guid UserId { get; set; }
    }
}
