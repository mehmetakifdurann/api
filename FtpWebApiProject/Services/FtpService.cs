using FluentFTP;
using Microsoft.Extensions.Options;

namespace FtpWebApiProject.Services
{
    // Dosya bilgilerini tutacak yeni modelimiz
    public class FtpFileItem
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public interface IFtpService
    {
        Task<bool> UploadFileAsync(IFormFile file, string remotePath);
        Task<List<FtpFileItem>> ListFilesAsync(string remotePath); // List<string> yerine List<FtpFileItem> yaptık
        Task<byte[]> DownloadFileAsync(string fileName);
    }

    public class FtpService : IFtpService
    {
        private readonly FtpSettings _settings;

        public FtpService(IOptions<FtpSettings> options)
        {
            _settings = options.Value;
        }

        private AsyncFtpClient CreateClient()
        {
            return new AsyncFtpClient(_settings.Host, _settings.User, _settings.Password, _settings.Port);
        }

        public async Task<bool> UploadFileAsync(IFormFile file, string remotePath)
        {
            using var client = CreateClient();
            await client.Connect();
            
            if (!remotePath.EndsWith("/")) remotePath += "/";
            string fullPath = remotePath + file.FileName;

            using var stream = file.OpenReadStream();
            var status = await client.UploadStream(stream, fullPath, FtpRemoteExists.Overwrite, true);
            
            await client.Disconnect();
            return status == FtpStatus.Success;
        }

        public async Task<List<FtpFileItem>> ListFilesAsync(string remotePath)
        {
            using var client = CreateClient();
            await client.Connect();
            
            var items = await client.GetListing(remotePath);
            
            // BURASI KRİTİK: Sadece dosyaları seçiyoruz (Klasörleri eliyoruz)
            // Ve detaylı bilgileri (Boyut, Tarih) alıyoruz.
            var fileList = items
                .Where(i => i.Type == FtpFileSystemObjectType.File) 
                .Select(i => new FtpFileItem 
                { 
                    Name = i.Name,
                    Size = i.Size,
                    ModifiedDate = i.Modified
                })
                .ToList();
            
            await client.Disconnect();
            return fileList;
        }

        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            using var client = CreateClient();
            await client.Connect();
            try 
            {
                var bytes = await client.DownloadBytes(fileName, CancellationToken.None);
                return bytes;
            }
            catch
            {
                return null; // Dosya yoksa veya hata olursa null döner
            }
            finally
            {
                await client.Disconnect();
            }
        }
    }
}