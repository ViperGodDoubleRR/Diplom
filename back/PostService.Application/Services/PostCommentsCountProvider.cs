using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetPostCommentsCounts;

namespace PostService.Application.Services
{
    public class PostCommentsCountProvider
    {
        private readonly IRpcClient _rpc;

        public PostCommentsCountProvider(IRpcClient rpc)
        {
            _rpc = rpc;
        }

        public async Task<Dictionary<Guid, int>> GetCountsAsync(
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken = default)
        {
            if (postIds.Count == 0)
                return new Dictionary<Guid, int>();

            try
            {
                var response = await _rpc.CallAsync<
                    GetPostCommentsCountsRpcRequest,
                    GetPostCommentsCountsRpcResponse>(
                    "comment.rpc",
                    new GetPostCommentsCountsRpcRequest
                    {
                        PostIds = postIds.ToList()
                    });

                return response?.Counts ?? new Dictionary<Guid, int>();
            }
            catch
            {
                return new Dictionary<Guid, int>();
            }
        }
    }
}
