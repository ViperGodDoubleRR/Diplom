using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Domain.Models
{
    public class ChatMedia
    {
        public int Id { get; set; }

        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        public string Bucket { get; set; } = null!;

        // 🔑 уникальный ключ файла в MinIO
        public string FileKey { get; set; } = null!;

        // 📄 оригинальное имя (для UI)
        public string OriginalName { get; set; } = null!;

        // 🧩 тип (avatar, post, video и т.д.)
        public string MediaType { get; set; } = null!;

        // 🎞 mime type
        public string ContentType { get; set; } = null!;

        // 📦 размер
        public long Size { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
