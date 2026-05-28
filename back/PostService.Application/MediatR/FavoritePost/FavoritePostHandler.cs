using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.FavoritePost
{
    public class FavoritePostHandler
     : IRequestHandler<FavoritePostCommand, ApiResponse<bool>>
    {
        private readonly IPostRepository _repository;

        public FavoritePostHandler(IPostRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<bool>> Handle(
            FavoritePostCommand request,
            CancellationToken cancellationToken)
        {
            await _repository.FavoritePostAsync(
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
