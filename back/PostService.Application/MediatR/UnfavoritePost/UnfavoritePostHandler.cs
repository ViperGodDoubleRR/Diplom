using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.UnfavoritePost
{
    public class UnfavoritePostHandler
    : IRequestHandler<UnfavoritePostCommand, ApiResponse<bool>>
    {
        private readonly IPostRepository _repository;

        public UnfavoritePostHandler(IPostRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<bool>> Handle(
            UnfavoritePostCommand request,
            CancellationToken cancellationToken)
        {
            await _repository.UnfavoritePostAsync(
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
