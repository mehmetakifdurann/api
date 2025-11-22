using FluentFTP;
using FtpWebApiProject.Models;
using Microsoft.Extensions.Options;

namespace FtpWebApiProject.Services
{
    public class FtpFileItem
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsFolder { get; set; }
    }

    public interface IFtpService
    {
        Task<bool> UploadFileAsync(IFormFile file, string remotePath);
        Task<List<FtpFileItem>> ListFilesAsync(string remotePath);
        Task<byte[]?> DownloadFileAsync(string fileName);
    }

    public class FtpService : IFtpService
    {
        private readonly FtpSettings _settings;

        public FtpService(IOptions<FtpSettings> options)
        {
            _settings = options.Value;
        }

        // DÜZELTME BURADA YAPILDI:
        private AsyncFtpClient CreateClient()
        {
            var client = new AsyncFtpClient(_settings.Host, _settings.User, _settings.Password, _settings.Port);
            
            // Config nesnesi oluşturuluyor
            client.Config.ConnectTimeout = 15000;
            client.Config.ReadTimeout = 15000;
            
            // Pasif mod ayarı için bu satırı kaldırdık (Varsayılan zaten Pasif'tir)
            // Eğer mutlaka belirtmek gerekirse Connect metodunda yapılır ama FluentFTP otomatiktir.
            // İlla zorlamak için eski versiyonlardaki kod yerine şu anki config yapısı yeterlidir.
            
            return client;
        }

        public async Task<bool> UploadFileAsync(IFormFile file, string remotePath)
        {
            using var client = CreateClient();
            // AutoConnect, en iyi bağlantı modunu (Pasif/Aktif) kendisi dener ve bulur.
            await client.AutoConnect(); 
            
            if (!remotePath.EndsWith("/")) remotePath += "/";
            string fullPath = remotePath + file.FileName;

            using var stream = file.OpenReadStream();
            try 
            {
                await client.UploadStream(stream, fullPath, FtpRemoteExists.Overwrite, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Upload Hatası: " + ex.Message);
                return false;
            }
            finally
            {
                await client.Disconnect();
            }
        }

        public async Task<List<FtpFileItem>> ListFilesAsync(string remotePath)
        {
            using var client = CreateClient();
            // AutoConnect kullanımı hayat kurtarır (DriveHQ için en iyisi)
            await client.AutoConnect();
            
            var items = await client.GetListing(remotePath);
            var fileList = new List<FtpFileItem>();

            foreach (var item in items)
            {
                // Tip kontrolü (Güncel FluentFTP sürümüne uygun)
                if (item.Type == FtpObjectType.File)
                {
                    fileList.Add(new FtpFileItem
                    {
                        Name = item.Name,
                        Size = item.Size,
                        ModifiedDate = item.Modified,
                        IsFolder = false
                    });
                }
            }
            
            await client.Disconnect();
            return fileList;
        }

        public async Task<byte[]?> DownloadFileAsync(string fileName)
        {
            using var client = CreateClient();
            await client.AutoConnect();
            
            try 
            {
                if (!await client.FileExists(fileName)) return null;
                var bytes = await client.DownloadBytes(fileName, CancellationToken.None);
                return bytes;
            }
            catch
            {
                return null;
            }
            finally
            {
                await client.Disconnect();
            }
        }
    }
}