using FluentFTP;
using FtpWebApiProject.Models; // Model klasörünün adı neyse onu yaz
using Microsoft.Extensions.Options;

namespace FtpWebApiProject.Services
{
    public interface IFtpService
    {
        Task<bool> UploadFileAsync(IFormFile file, string remotePath);
        Task<List<string>> ListFilesAsync(string remotePath); // Basit listeleme
        Task<byte[]> DownloadFileAsync(string fileName); // İndirme
    }

    public class FtpService : IFtpService
    {
        private readonly FtpSettings _settings;

        public FtpService(IOptions<FtpSettings> options)
        {
            _settings = options.Value;
        }

        // Yardımcı metot: Bağlantı oluşturur
        private AsyncFtpClient CreateClient()
        {
            return new AsyncFtpClient(_settings.Host, _settings.User, _settings.Password, _settings.Port);
        }

        public async Task<bool> UploadFileAsync(IFormFile file, string remotePath)
        {
            using var client = CreateClient();
            await client.Connect();
            
            using var stream = file.OpenReadStream();
            // Dosyayı yükle (Üstüne yazar)
            var status = await client.UploadStream(stream, remotePath + file.FileName, FtpRemoteExists.Overwrite, true);
            
            await client.Disconnect();
            return status == FtpStatus.Success;
        }

        public async Task<List<string>> ListFilesAsync(string remotePath)
        {
            using var client = CreateClient();
            await client.Connect();
            
            var items = await client.GetListing(remotePath);
            var fileNames = items.Select(i => i.Name).ToList();
            
            await client.Disconnect();
            return fileNames;
        }

        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            using var client = CreateClient();
            await client.Connect();

            // Dosyayı byte array olarak indir
            var bytes = await client.DownloadBytes(fileName, CancellationToken.None);
            
            await client.Disconnect();
            return bytes;
        }
    }
}