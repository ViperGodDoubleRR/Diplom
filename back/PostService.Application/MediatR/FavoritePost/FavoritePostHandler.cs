using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.FavoritePost
{
    public class FavoritePostHandler : IRequestHandler<FavoritePostCommand, ApiResponse<bool>>
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
            if (!await _repository.ExistsActiveAsync(request.PostId, cancellationToken))
            {
                return Fail("POST_NOT_FOUND", "Пост не найден");
            }

            await _repository.FavoritePostAsync(request.PostId, request.UserId, cancellationToken);

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
