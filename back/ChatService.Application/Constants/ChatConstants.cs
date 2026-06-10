namespace ChatService.Application.Constants
{
    public static class ChatConstants
    {
        public const int MessagePageSize = 50;
        public const int SearchGroupsLimit = 30;
        public const int MaxGroupNameLength = 255;
        public const int MaxMessageLength = 4000;
        public const long MaxMediaBytes = 300L * 1024 * 1024;
        public const long MaxGalleryImageBytes = 5L * 1024 * 1024;
        public const long MaxGalleryVideoBytes = 30L * 1024 * 1024;
    }
}
