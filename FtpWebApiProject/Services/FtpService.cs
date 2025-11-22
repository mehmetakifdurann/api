using FluentFTP;
using Microsoft.Extensions.Options;

namespace FtpWebApiProject.Services
{
    public class FtpFileItem
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsFolder { get; set; } // Klasör mü kontrolü
    }

    public interface IFtpService
    {
        Task<bool> UploadFileAsync(IFormFile file, string remotePath);
        Task<List<FtpFileItem>> ListFilesAsync(string remotePath);
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
            
            // Yol düzeltme
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
            await client.Connect();
            
            // Tüm listeyi al
            var items = await client.GetListing(remotePath);
            
            var fileList = new List<FtpFileItem>();

            foreach (var item in items)
            {
                // Sadece DOSYALARI al (Type == File)
                if (item.Type == FtpFileSystemObjectType.File)
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

        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            using var client = CreateClient();
            await client.Connect();
            
            Console.WriteLine($"İndirme İsteği Geldi: {fileName}");

            try 
            {
                // Dosya var mı kontrol et
                bool exists = await client.FileExists(fileName);
                if (!exists) 
                {
                    Console.WriteLine("Dosya FTP'de bulunamadı!");
                    return null;
                }

                // İndir
                var bytes = await client.DownloadBytes(fileName, CancellationToken.None);
                Console.WriteLine($"İndirme Başarılı: {bytes.Length} byte");
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FTP İndirme Hatası: " + ex.Message);
                return null;
            }
            finally
            {
                await client.Disconnect();
            }
        }
    }
}