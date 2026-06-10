namespace PostService.Application.DTO
{
    public class UpdatePostRequest
    {
        public string Description { get; set; } = string.Empty;

        public List<PostMediaDto> Media { get; set; } = [];
    }
}
