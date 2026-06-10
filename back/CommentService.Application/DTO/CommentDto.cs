namespace CommentService.Application.DTO
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid? ParentId { get; set; }
        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public CommentUserDto User { get; set; } = null!;
        public List<CommentMediaDto> Media { get; set; } = [];
        public CommentReactionSummaryDto Reactions { get; set; } = new();
        public int RepliesCount { get; set; }
    }
}
