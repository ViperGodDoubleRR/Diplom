namespace CommentService.Application.DTO
{
    public class GetPostCommentsResponse
    {
        public List<CommentDto> Items { get; set; } = [];
        /// <summary>Корневые комментарии (для пагинации списка).</summary>
        public int TotalCount { get; set; }
        /// <summary>Все комментарии поста, включая ответы.</summary>
        public int AllCommentsCount { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public bool HasMore { get; set; }
    }
}
