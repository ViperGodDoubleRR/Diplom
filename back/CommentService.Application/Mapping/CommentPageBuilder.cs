using CommentService.Application.DTO;
using CommentService.Application.Mapping;
using CommentService.Application.Validation;
using CommentService.Domain.IRepository;
using CommentService.Domain.Models;

using Shared.MinIO.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetUserPost;

namespace CommentService.Application.Mapping
{
    public class CommentPageBuilder
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICommentMediaRepository _mediaRepository;
        private readonly IMinioService _minio;
        private readonly IRpcClient _rpc;

        public CommentPageBuilder(
            ICommentRepository commentRepository,
            ICommentMediaRepository mediaRepository,
            IMinioService minio,
            IRpcClient rpc)
        {
            _commentRepository = commentRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
            _rpc = rpc;
        }

        public async Task<GetPostCommentsResponse> BuildPageAsync(
            IReadOnlyList<Comment> pageComments,
            int totalCount,
            int allCommentsCount,
            int offset,
            int limit,
            Guid currentUserId,
            bool includeRepliesCount,
            CancellationToken cancellationToken)
        {
            if (pageComments.Count == 0)
            {
                return new GetPostCommentsResponse
                {
                    Items = [],
                    TotalCount = totalCount,
                    AllCommentsCount = allCommentsCount,
                    Offset = offset,
                    Limit = limit,
                    HasMore = false
                };
            }

            var commentIds = pageComments.Select(x => x.Id).ToList();
            var reactionsByComment = await _commentRepository.GetReactionsByCommentIdsAsync(
                commentIds,
                cancellationToken);
            var mediaByComment = await _mediaRepository.GetByCommentIdsAsync(
                commentIds,
                cancellationToken);

            var userIds = pageComments.Select(x => x.UserId).Distinct().ToList();
            var users = new Dictionary<Guid, GetUserRpcResponse?>();

            foreach (var userId in userIds)
            {
                var user = await _rpc.CallAsync<GetUserRpcRequest, GetUserRpcResponse>(
                    "user.rpc",
                    new GetUserRpcRequest { UserId = userId });

                users[userId] = user;
            }

            var result = new List<CommentDto>();

            foreach (var comment in pageComments)
            {
                var mediaDtos = new List<CommentMediaDto>();

                if (mediaByComment.TryGetValue(comment.Id, out var media))
                {
                    mediaDtos.Add(await CommentMapper.ToMediaDtoAsync(media, _minio, cancellationToken));
                }

                reactionsByComment.TryGetValue(comment.Id, out var reactions);

                result.Add(new CommentDto
                {
                    Id = comment.Id,
                    PostId = comment.PostId,
                    ParentId = comment.ParentId,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    IsDeleted = comment.IsDeleted,
                    User = CommentMapper.ToUserDto(users.GetValueOrDefault(comment.UserId), comment.UserId),
                    Media = mediaDtos,
                    Reactions = CommentMapper.ToReactionSummary(
                        reactions ?? [],
                        currentUserId),
                    RepliesCount = includeRepliesCount
                        ? await _commentRepository.GetRepliesCountAsync(comment.Id, cancellationToken)
                        : 0
                });
            }

            return new GetPostCommentsResponse
            {
                Items = result,
                TotalCount = totalCount,
                AllCommentsCount = allCommentsCount,
                Offset = offset,
                Limit = limit,
                HasMore = offset + result.Count < totalCount
            };
        }

        public static (int offset, int limit) NormalizePaging(int offset, int limit) =>
            CommentValidation.NormalizeOffsetPaging(offset, limit);
    }
}
