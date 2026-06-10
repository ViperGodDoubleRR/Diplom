using ChatService.Application.DTO;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace ChatService.Application.MediatR.InviteGroupMember
{
    public class InviteGroupMemberCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public Guid TargetUserId { get; set; }
    }

    public class InviteGroupMemberHandler
        : IRequestHandler<InviteGroupMemberCommand, ApiResponse<bool>>
    {
        private readonly IChatRepository _chatRepository;

        public InviteGroupMemberHandler(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ApiResponse<bool>> Handle(
            InviteGroupMemberCommand request,
            CancellationToken cancellationToken)
        {
            if (request.UserId == request.TargetUserId)
                return Fail("INVALID_TARGET", "Нельзя пригласить самого себя");

            var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);

            if (chat is null || chat.Type != ChatType.Group)
                return Fail("CHAT_NOT_FOUND", "Группа не найдена");

            var role = await _chatRepository.GetMemberRoleAsync(
                request.ChatId,
                request.UserId,
                cancellationToken);

            if (role != ChatRole.Admin)
                return Fail("FORBIDDEN", "Только админ может приглашать участников");

            if (await _chatRepository.IsMemberAsync(request.ChatId, request.TargetUserId, cancellationToken))
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true
                };
            }

            await _chatRepository.AddMemberAsync(new ChatUser
            {
                ChatId = request.ChatId,
                UserId = request.TargetUserId,
                Role = ChatRole.Member,
                JoinedAt = DateTime.UtcNow
            }, cancellationToken);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
