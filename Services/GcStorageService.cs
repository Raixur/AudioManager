using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AudioManager.Services;
using Google;
using Google.Apis.Download;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace GCAudioManager.Services
{
    public class GcStorageOptions
    {
        public string ProjectId { get; set; }
    }

    public class GcStorageService : IStorageService
    {
        private readonly StorageClient _client;
        private readonly string _projectId;

        public GcStorageService(IOptions<GcStorageOptions> options)
        {
            _client = StorageClient.Create();
            _projectId = options.Value.ProjectId;
        }

        public bool Exists(string bucket, string fileName)
        {
            try
            {
                var list = _client.ListObjects(bucket, fileName);
                return list.Any();
            }
            catch (GoogleApiException e)
            {
                // TODO: add exception code check
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string bucket, string fileName)
        {
            try
            {
                var list = _client.ListObjectsAsync(bucket, fileName);
                return await list.Any();
            }
            catch (GoogleApiException e)
            {
                // TODO: add exception code check
                Console.WriteLine(e);
                throw;
            }
        }


        public bool BucketExists(string bucket)
        {
            try
            {
                var buckets = _client.ListBuckets(_projectId, new ListBucketsOptions { Prefix = bucket });
                return buckets.Any();
            }
            catch (GoogleApiException e)
            {
                // TODO: add exception code check
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> BucketExistsAsync(string bucket)
        {
            try
            {
                var buckets = _client.ListBucketsAsync(_projectId, new ListBucketsOptions { Prefix = bucket });
                return await buckets.Any();
            }
            catch (GoogleApiException e)
            {
                // TODO: add exception code check
                Console.WriteLine(e);
                throw;
            }   
        }


        public bool CreateBucket(string bucket)
        {
            try
            {
                _client.CreateBucket(_projectId, bucket);
                return true;
            }
            catch (GoogleApiException e)
            {
                // Bucket already exist
                return e.HttpStatusCode == HttpStatusCode.Conflict;
            }
        }

        public async Task<bool> CreateBucketAsync(string bucket)
        {
            try
            {
                await _client.CreateBucketAsync(_projectId, bucket);
                return true;
            }
            catch (GoogleApiException e)
            {
                // Bucket already exist
                return e.HttpStatusCode == HttpStatusCode.Conflict;
            }
        }


        public async Task UploadAsync(string bucket, string source, string destination, 
                                      IProgress<IUploadProgress> progress = null)
        {
            var contentType = GetContentType(source);
            if (contentType == "")
            {
                throw new ArgumentException("Invalid content type");
            }

            if (!File.Exists(source))
            {
                throw new FileNotFoundException("File not found", source);
            }

            try
            {
                using (var stream = new FileStream(source, FileMode.Open))
                {
                    await _client.UploadObjectAsync(bucket, destination, contentType, stream, progress: progress);
                }
            }
            catch (GoogleApiException e)
            {
                // TODO: add exception code check
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task DownloadAsync(string bucket, string source, string destination,
                                        IProgress<IDownloadProgress> progress)
        {
            try
            {
                using (var stream = new FileStream(destination, FileMode.Create))
                {
                    await _client.DownloadObjectAsync(bucket, source, stream, progress: progress);
                }
            }
            catch (GoogleApiException e)
            {
                // TODO: add exception code check
                Console.WriteLine(e);
                throw;
            }
        }

        private static string GetContentType(string fileName)
        {
            string contentType;
            var extension = Path.GetExtension(fileName);

            return ContentMappings.TryGetValue(extension, out contentType) ? contentType : "";
        }

        private static readonly IDictionary<string, string> ContentMappings = new Dictionary<string, string> {
            #region Big freaking list of mime types
            {".aa",   "audio/audible"},
            {".AAC",  "audio/aac"},
            {".aax",  "audio/vnd.audible.aax"},
            {".ac3",  "audio/ac3"},
            {".ADT",  "audio/vnd.dlna.adts"},
            {".ADTS", "audio/aac"},
            {".aif",  "audio/x-aiff"},
            {".aifc", "audio/aiff"},
            {".aiff", "audio/aiff"},
            {".au",   "audio/basic"},
            {".caf",  "audio/x-caf"},
            {".cdda", "audio/aiff"},
            {".gsm",  "audio/x-gsm"},
            {".m3u",  "audio/x-mpegurl"},
            {".m3u8", "audio/x-mpegurl"},
            {".m4a",  "audio/m4a"},
            {".m4b",  "audio/m4b"},
            {".m4p",  "audio/m4p"},
            {".m4r",  "audio/x-m4r"},
            {".mid",  "audio/mid"},
            {".midi", "audio/mid"},
            {".mp3",  "audio/mpeg"},
            {".pls",  "audio/scpls"},
            {".ra",   "audio/x-pn-realaudio"},
            {".ram",  "audio/x-pn-realaudio"},
            {".rmi",  "audio/mid"},
            {".rpm",  "audio/x-pn-realaudio-plugin"},
            {".sd2",  "audio/x-sd2"},
            {".smd",  "audio/x-smd"},
            {".smx",  "audio/x-smd"},
            {".smz",  "audio/x-smd"},
            {".snd",  "audio/basic"},
            {".wav",  "audio/wav"},
            {".wave", "audio/wav"},
            {".wax",  "audio/x-ms-wax"},
            {".wma",  "audio/x-ms-wma"}
            #endregion
        };

    }
}