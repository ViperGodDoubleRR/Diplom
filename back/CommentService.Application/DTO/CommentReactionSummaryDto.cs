namespace CommentService.Application.DTO
{
    public class CommentReactionSummaryDto
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int Loves { get; set; }
        public int Angry { get; set; }
        public int? MyReaction { get; set; }
    }
}
