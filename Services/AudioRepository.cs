using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlashFP;
using FlashFP.AudioProcessing;
using Microsoft.Extensions.Options;

namespace AudioManager.Services
{
    public class AudioRepositoryOptions
    {
        public string AudioDir { get; set; }
    }
    
    public class AudioRepository
    {
        private readonly IStorageService _cloudStorage;
        private readonly FlashFp _flash;
        private readonly string _audioDir;

        public AudioRepository(IStorageService cloudStorage, FlashFp flash, IOptions<AudioRepositoryOptions> options)
        {
            _cloudStorage = cloudStorage;
            _flash = flash;
            _audioDir = options.Value.AudioDir;
        }

        public async Task<bool> RegisterAudio(AudioInfo audio, string path)
        {
            var id = path.GetHashCode();
            var audioPath = GetAudioPath(path);
            var registered = _flash.Store(id, audioPath, audio);

            var destinationPath = Path.Combine(audio.Album, audio.Title);
            await _cloudStorage.UploadAsync(audio.Author, audioPath, destinationPath);

            return registered;
        }

        public AudioInfo GetAudio(string path)
        {
            var id = _flash.Query(path).Identifier;
            return _flash.GetAudioDescription(id);
        }

        public IEnumerable<AudioInfo> GetAduioList(string path)
        {
            var results = _flash.QueryList(path);
            return results.Select(qr => _flash.GetAudioDescription(qr.Identifier));
        }


        public void SaveAudio(Stream stream, string fileName)
        {
            var destinationPath = Path.Combine(_audioDir, fileName);
            using (var fileStream = new FileStream(destinationPath, FileMode.Create))
            {
                stream.CopyTo(fileStream);
            }
        }

        public string GetAudioPath(string audio)
        {
            var audioFile = Path.Combine(_audioDir, audio);
            return File.Exists(audioFile) ? audioFile : null;
        }       


        public async Task UploadAsync(string bucket, string fileName)
        {
            var localPath = Path.Combine(_audioDir, bucket, fileName);
            if (File.Exists(localPath))
            {
                await _cloudStorage.UploadAsync(bucket, localPath, fileName);
            }
        }

        public async Task DownloadAsync(string bucket, string fileName)
        {
            var localPath = Path.Combine(_audioDir, bucket, fileName);
            if (!File.Exists(localPath))
            {
                await _cloudStorage.DownloadAsync(bucket, fileName, localPath);
            }
        }
    }
}