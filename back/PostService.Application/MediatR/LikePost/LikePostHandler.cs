using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.LikePost
{
    public class LikePostHandler
     : IRequestHandler<LikePostCommand, ApiResponse<bool>>
    {
        private readonly IPostRepository _repository;

        public LikePostHandler(IPostRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<bool>> Handle(
            LikePostCommand request,
            CancellationToken cancellationToken)
        {
            await _repository.LikePostAsync(
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
