using CommentService.Domain.IRepository;

using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetPostCommentsCounts;

namespace CommentService.Application.RPC
{
    public class GetPostCommentsCountsRpcHandler
        : IRPCHandle<GetPostCommentsCountsRpcRequest, GetPostCommentsCountsRpcResponse>
    {
        private readonly ICommentRepository _commentRepository;

        public GetPostCommentsCountsRpcHandler(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<GetPostCommentsCountsRpcResponse> Handle(
            GetPostCommentsCountsRpcRequest request)
        {
            var counts = await _commentRepository.CountAllByPostIdsAsync(request.PostIds);

            return new GetPostCommentsCountsRpcResponse
            {
                Counts = counts
            };
        }
    }
}
