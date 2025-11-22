using FluentFTP;
using Microsoft.Extensions.Options;

namespace FtpWebApiProject.Services
{
    public class FtpService : IFtpService
    {
        private readonly string _host;
        private readonly string _user;
        private readonly string _pass;
        private readonly int _port;

        // appsettings.json'dan verileri çekiyoruz
        public FtpService(IConfiguration config)
        {
            _host = config["FtpSettings:Host"]!;
            _user = config["FtpSettings:User"]!;
            _pass = config["FtpSettings:Password"]!;
            _port = int.Parse(config["FtpSettings:Port"]!);
        }

        public async Task<bool> UploadFileAsync(IFormFile file, string remotePath)
        {
            try
            {
                // FTP İstemcisini oluştur
                using var client = new AsyncFtpClient(_host, _user, _pass, _port);
                
                // Bağlan (Otomatik olarak SSL/TLS dener, smartFTP mantığı)
                await client.Connect();

                // Gelen dosyanın içeriğini Stream (akış) olarak al
                using var stream = file.OpenReadStream();

                // Yükleme işlemi:
                // remotePath: "/public_html/resimler/resim.jpg" gibi olmalı
                string fullPath = remotePath + "/" + file.FileName;

                // Dosyayı yükle. FtpRemoteExists.Overwrite, dosya varsa üzerine yazar.
                var status = await client.UploadStream(stream, fullPath, FtpRemoteExists.Overwrite, true);

                await client.Disconnect();

                return status == FtpStatus.Success;
            }
            catch (Exception ex)
            {
                // Hata olursa loglayabilirsin
                Console.WriteLine($"FTP Hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string remotePath)
        {
            using var client = new AsyncFtpClient(_host, _user, _pass, _port);
            await client.Connect();
            
            var items = await client.GetListing(remotePath);
            await client.Disconnect();

            return items.Select(x => x.Name);
        }
    }
}