using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Models
{
    public class MediaUser
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // 🪣 где лежит файл
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
