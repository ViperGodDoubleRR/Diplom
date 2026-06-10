namespace ChatService.Application.DTO
{
    public class GetMessagesResponse
    {
        public List<MessageDto> Items { get; set; } = [];
        public bool HasMore { get; set; }
    }
}
