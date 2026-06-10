using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace ChatService.Application.MediatR.RemoveGroupMember
{
    public class RemoveGroupMemberCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public Guid TargetUserId { get; set; }
    }

    public class RemoveGroupMemberHandler
        : IRequestHandler<RemoveGroupMemberCommand, ApiResponse<bool>>
    {
        private readonly IChatRepository _chatRepository;

        public RemoveGroupMemberHandler(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ApiResponse<bool>> Handle(
            RemoveGroupMemberCommand request,
            CancellationToken cancellationToken)
        {
            if (request.UserId == request.TargetUserId)
                return Fail("INVALID_TARGET", "Нельзя удалить самого себя");

            var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);

            if (chat is null || chat.Type != ChatType.Group)
                return Fail("CHAT_NOT_FOUND", "Группа не найдена");

            var role = await _chatRepository.GetMemberRoleAsync(
                request.ChatId,
                request.UserId,
                cancellationToken);

            if (role != ChatRole.Admin)
                return Fail("FORBIDDEN", "Только админ может удалять участников");

            var target = chat.Users.FirstOrDefault(u => u.UserId == request.TargetUserId);
            if (target is null)
                return Fail("MEMBER_NOT_FOUND", "Участник не найден");

            var adminCount = chat.Users.Count(u => u.Role == ChatRole.Admin);
            if (target.Role == ChatRole.Admin && adminCount <= 1)
                return Fail("LAST_ADMIN", "Нельзя удалить последнего администратора");

            await _chatRepository.RemoveMemberAsync(
                request.ChatId,
                request.TargetUserId,
                cancellationToken);

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
