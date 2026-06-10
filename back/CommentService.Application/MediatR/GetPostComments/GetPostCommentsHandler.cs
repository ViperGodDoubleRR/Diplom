using CommentService.Application.DTO;
using CommentService.Application.Mapping;
using CommentService.Application.Validation;
using CommentService.Domain.IRepository;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;

namespace CommentService.Application.MediatR.GetPostComments
{
    public class GetPostCommentsHandler
        : IRequestHandler<GetPostCommentsQuery, ApiResponse<GetPostCommentsResponse>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly CommentPageBuilder _pageBuilder;

        public GetPostCommentsHandler(
            ICommentRepository commentRepository,
            ICommentMediaRepository mediaRepository,
            IMinioService minio,
            IRpcClient rpc)
        {
            _commentRepository = commentRepository;
            _pageBuilder = new CommentPageBuilder(commentRepository, mediaRepository, minio, rpc);
        }

        public async Task<ApiResponse<GetPostCommentsResponse>> Handle(
            GetPostCommentsQuery request,
            CancellationToken cancellationToken)
        {
            var (offset, limit) = CommentPageBuilder.NormalizePaging(request.Offset, request.Limit);

            var totalCount = await _commentRepository.CountRootByPostIdAsync(
                request.PostId,
                cancellationToken);

            var allCommentsCount = await _commentRepository.CountAllByPostIdAsync(
                request.PostId,
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

            var pageComments = await _commentRepository.GetRootCommentsByPostIdAsync(
                request.PostId,
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
                includeRepliesCount: true,
                cancellationToken);

            return new ApiResponse<GetPostCommentsResponse>
            {
                Success = true,
                Data = data
            };
        }
    }
}
