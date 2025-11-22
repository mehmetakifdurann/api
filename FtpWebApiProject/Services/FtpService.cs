using FluentFTP;
using FtpWebApiProject.Models;
using Microsoft.Extensions.Options;

namespace FtpWebApiProject.Services
{
    public interface IFtpService
    {
        Task<bool> UploadFileAsync(IFormFile file, string remotePath);
        Task<List<string>> ListFilesAsync(string remotePath);
        Task<byte[]> DownloadFileAsync(string fileName);
    }

    public class FtpService : IFtpService
    {
        private readonly FtpSettings _settings;

        public FtpService(IOptions<FtpSettings> options)
        {
            _settings = options.Value;
        }

        // Yardımcı metot: Bağlantı ayarları
        private AsyncFtpClient CreateClient()
        {
            return new AsyncFtpClient(_settings.Host, _settings.User, _settings.Password, _settings.Port);
        }

        public async Task<bool> UploadFileAsync(IFormFile file, string remotePath)
        {
            using var client = CreateClient();
            await client.Connect(); // Sunucuya bağlan
            
            // Yolun sonuna "/" eklenmemişse biz ekleyelim ki dosya adı karışmasın
            if (!remotePath.EndsWith("/")) remotePath += "/";
            string fullPath = remotePath + file.FileName;

            using var stream = file.OpenReadStream();
            
            // Dosyayı yükle (Mevcut varsa üstüne yazar - Overwrite)
            var status = await client.UploadStream(stream, fullPath, FtpRemoteExists.Overwrite, true);
            
            await client.Disconnect();
            return status == FtpStatus.Success;
        }

        public async Task<List<string>> ListFilesAsync(string remotePath)
        {
            using var client = CreateClient();
            await client.Connect();
            
            // İyileştirme: Sadece dosya isimlerini al (Klasörleri filtrele)
            var items = await client.GetListing(remotePath);
            
            var fileNames = items
                .Where(i => i.Type == FtpFileSystemObjectType.File) // Sadece dosyalar!
                .Select(i => i.Name)
                .ToList();
            
            await client.Disconnect();
            return fileNames;
        }

        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            using var client = CreateClient();
            await client.Connect();

            // Dosyayı indirip byte array olarak döndür
            var bytes = await client.DownloadBytes(fileName, CancellationToken.None);
            
            await client.Disconnect();
            return bytes;
        }
    }
}