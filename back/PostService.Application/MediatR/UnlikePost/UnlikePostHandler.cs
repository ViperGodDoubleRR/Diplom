using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.UnlikePost
{
    public class UnlikePostHandler
     : IRequestHandler<UnlikePostCommand, ApiResponse<bool>>
    {
        private readonly IPostRepository _repository;

        public UnlikePostHandler(IPostRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<bool>> Handle(
            UnlikePostCommand request,
            CancellationToken cancellationToken)
        {
            await _repository.UnlikePostAsync(
                request.PostId,
                request.UserId
            );

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}
