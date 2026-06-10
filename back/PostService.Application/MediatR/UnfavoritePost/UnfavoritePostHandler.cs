using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.UnfavoritePost
{
    public class UnfavoritePostHandler : IRequestHandler<UnfavoritePostCommand, ApiResponse<bool>>
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
            if (!await _repository.ExistsActiveAsync(request.PostId, cancellationToken))
            {
                return Fail("POST_NOT_FOUND", "Пост не найден");
            }

            await _repository.UnfavoritePostAsync(request.PostId, request.UserId, cancellationToken);

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
