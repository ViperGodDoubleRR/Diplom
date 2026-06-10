using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.LikePost
{
    public class LikePostHandler : IRequestHandler<LikePostCommand, ApiResponse<bool>>
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
            if (!await _repository.ExistsActiveAsync(request.PostId, cancellationToken))
            {
                return Fail("POST_NOT_FOUND", "Пост не найден");
            }

            await _repository.LikePostAsync(request.PostId, request.UserId, cancellationToken);

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
