using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.MinIO.Models;

namespace Shared.MinIO.Interfaces
{
    public interface IMinioService
    {
        Task EnsureBucketAsync(string bucket, CancellationToken cancellationToken = default);
        Task<UploadFileResult> UploadFileAsync(Stream file, string fileName, string contentType, string bucket);
        Task DeleteFileAsync(string fileName, string bucket);
        Task<string> GetFileUrlAsync(string fileName, string bucket);
    }
}
