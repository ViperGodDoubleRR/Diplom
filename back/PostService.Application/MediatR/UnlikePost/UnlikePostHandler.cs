using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.UnlikePost
{
    public class UnlikePostHandler : IRequestHandler<UnlikePostCommand, ApiResponse<bool>>
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
            if (!await _repository.ExistsActiveAsync(request.PostId, cancellationToken))
            {
                return Fail("POST_NOT_FOUND", "Пост не найден");
            }

            await _repository.UnlikePostAsync(request.PostId, request.UserId, cancellationToken);

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
