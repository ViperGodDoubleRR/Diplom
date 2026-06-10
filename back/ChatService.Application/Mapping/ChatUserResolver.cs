using ChatService.Application.DTO;
using ChatService.Domain.Models;

using Shared.MinIO.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetUserPost;

namespace ChatService.Application.Mapping
{
    public class ChatUserResolver
    {
        private readonly IRpcClient _rpc;
        private readonly Dictionary<Guid, ChatUserDto> _cache = new();

        public ChatUserResolver(IRpcClient rpc)
        {
            _rpc = rpc;
        }

        public async Task<ChatUserDto> ResolveAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(userId, out var cached))
                return cached;

            GetUserRpcResponse? user = null;

            try
            {
                user = await _rpc.CallAsync<GetUserRpcRequest, GetUserRpcResponse>(
                    "user.rpc",
                    new GetUserRpcRequest { UserId = userId });
            }
            catch
            {
                user = null;
            }

            var dto = new ChatUserDto
            {
                Id = userId,
                Login = user?.Login ?? "User",
                Tag = user?.Tag ?? string.Empty,
                Avatar = user?.AvatarUrl ?? string.Empty,
                AvatarIsVideo = user?.AvatarIsVideo ?? false
            };

            _cache[userId] = dto;
            return dto;
        }

        public async Task<Dictionary<Guid, ChatUserDto>> ResolveManyAsync(
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, ChatUserDto>();

            foreach (var userId in userIds.Distinct())
            {
                result[userId] = await ResolveAsync(userId, cancellationToken);
            }

            return result;
        }
    }
}
