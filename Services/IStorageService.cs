using System;
using System.Threading.Tasks;
using Google.Apis.Download;
using Google.Apis.Upload;

namespace AudioManager.Services
{
    public interface IStorageService
    {
        bool Exists(string bucket, string fileName);
        Task<bool> ExistsAsync(string bucket, string fileName);

        bool BucketExists(string bucket);
        Task<bool> BucketExistsAsync(string bucket);

        bool CreateBucket(string bucket);
        Task<bool> CreateBucketAsync(string bucket);

        Task DownloadAsync(string bucket, string source, string destination, IProgress<IDownloadProgress> progress = null);
        Task UploadAsync(string bucket, string source, string destination, IProgress<IUploadProgress> progress = null);
    }
}