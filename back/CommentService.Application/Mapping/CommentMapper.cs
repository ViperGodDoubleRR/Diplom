using CommentService.Application.DTO;
using CommentService.Domain.Models;

using Shared.MinIO.Interfaces;
using Shared.RabbitMQ.rpc.Contracts.GetUserPost;

namespace CommentService.Application.Mapping
{
    public static class CommentMapper
    {
        public static async Task<CommentMediaDto> ToMediaDtoAsync(
            CommentMedia media,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var url = await minio.GetFileUrlAsync(media.FileKey, media.Bucket);

            return new CommentMediaDto
            {
                Id = media.Id,
                Url = url,
                MediaType = media.MediaType,
                OriginalName = media.OriginalName
            };
        }

        public static CommentReactionSummaryDto ToReactionSummary(
            IEnumerable<CommentReaction> reactions,
            Guid currentUserId)
        {
            var list = reactions.ToList();
            var mine = list.FirstOrDefault(x => x.UserId == currentUserId);

            return new CommentReactionSummaryDto
            {
                Likes = list.Count(x => x.Type == ReactionType.Like),
                Dislikes = list.Count(x => x.Type == ReactionType.Dislike),
                Loves = list.Count(x => x.Type == ReactionType.Love),
                Angry = list.Count(x => x.Type == ReactionType.Angry),
                MyReaction = mine is null ? null : (int)mine.Type
            };
        }

        public static CommentUserDto ToUserDto(GetUserRpcResponse? user, Guid userId) =>
            new()
            {
                Id = userId,
                Login = user?.Login ?? "User",
                Tag = user?.Tag ?? string.Empty,
                Avatar = user?.AvatarUrl ?? string.Empty
            };
    }
}
