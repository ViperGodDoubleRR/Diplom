using CommentService.Application.DTO;
using CommentService.Application.Mapping;
using CommentService.Domain.IRepository;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;

namespace CommentService.Application.MediatR.GetCommentReplies
{
    public class GetCommentRepliesHandler
        : IRequestHandler<GetCommentRepliesQuery, ApiResponse<GetPostCommentsResponse>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly CommentPageBuilder _pageBuilder;

        public GetCommentRepliesHandler(
            ICommentRepository commentRepository,
            ICommentMediaRepository mediaRepository,
            IMinioService minio,
            IRpcClient rpc)
        {
            _commentRepository = commentRepository;
            _pageBuilder = new CommentPageBuilder(commentRepository, mediaRepository, minio, rpc);
        }

        public async Task<ApiResponse<GetPostCommentsResponse>> Handle(
            GetCommentRepliesQuery request,
            CancellationToken cancellationToken)
        {
            var parent = await _commentRepository.GetByIdAsync(
                request.ParentCommentId,
                cancellationToken);

            if (parent is null || parent.IsDeleted)
            {
                return new ApiResponse<GetPostCommentsResponse>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "COMMENT_NOT_FOUND",
                        Message = "Комментарий не найден"
                    }
                };
            }

            var (offset, limit) = CommentPageBuilder.NormalizePaging(request.Offset, request.Limit);
            var totalCount = await _commentRepository.GetRepliesCountAsync(
                request.ParentCommentId,
                cancellationToken);

            var allCommentsCount = await _commentRepository.CountAllByPostIdAsync(
                parent.PostId,
                cancellationToken);

            if (totalCount == 0 || offset >= totalCount)
            {
                return new ApiResponse<GetPostCommentsResponse>
                {
                    Success = true,
                    Data = new GetPostCommentsResponse
                    {
                        Items = [],
                        TotalCount = totalCount,
                        AllCommentsCount = allCommentsCount,
                        Offset = offset,
                        Limit = limit,
                        HasMore = false
                    }
                };
            }

            var pageComments = await _commentRepository.GetRepliesAsync(
                request.ParentCommentId,
                offset,
                limit,
                cancellationToken);

            var data = await _pageBuilder.BuildPageAsync(
                pageComments,
                totalCount,
                allCommentsCount,
                offset,
                limit,
                request.CurrentUserId,
                includeRepliesCount: false,
                cancellationToken);

            return new ApiResponse<GetPostCommentsResponse>
            {
                Success = true,
                Data = data
            };
        }
    }
}
