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

        // BAĞLANTI AYARLARI BURADA YAPILIYOR
        private AsyncFtpClient CreateClient()
        {
            var client = new AsyncFtpClient(_settings.Host, _settings.User, _settings.Password, _settings.Port);
            
            // ÖNEMLİ: DriveHQ ve Bulut sunucular için PASV (Pasif) modu zorunludur.
            // Bunu açmazsan bağlantı kurulur ama dosya listesi gelmez (donar).
            client.Config.DataConnectionMode = FtpDataConnectionMode.PASV;
            
            // Bağlantı zaman aşımını biraz uzatalım (15 saniye)
            client.Config.ConnectTimeout = 15000;
            client.Config.ReadTimeout = 15000;

            return client;
        }

        public async Task<bool> UploadFileAsync(IFormFile file, string remotePath)
        {
            using var client = CreateClient();
            await client.Connect(); // Bağlan
            
            // Yolun sonuna / ekle ki dosya adıyla yapışmasın
            if (!remotePath.EndsWith("/")) remotePath += "/";
            string fullPath = remotePath + file.FileName;

            using var stream = file.OpenReadStream();
            try 
            {
                // Dosyayı yükle (Varsa üzerine yaz)
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
            await client.Connect();
            
            // Tüm listeyi çek
            var items = await client.GetListing(remotePath);
            
            var fileList = new List<FtpFileItem>();

            foreach (var item in items)
            {
                // Sadece DOSYALARI filtrele (Klasörleri gösterme)
                // FluentFTP yeni sürümlerinde FtpObjectType kullanılır
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
            await client.Connect();
            
            try 
            {
                // Dosya var mı kontrol et
                if (!await client.FileExists(fileName)) return null;

                // Dosyayı indir
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