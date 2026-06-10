using MediatR;

using PostService.Application.DTO;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.UpdatePost
{
    public class UpdatePostCommand : IRequest<ApiResponse<bool>>
    {
        public Guid PostId { get; set; }

        public Guid UserId { get; set; }

        public UpdatePostRequest Request { get; set; } = null!;
    }
}
