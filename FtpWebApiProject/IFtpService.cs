using FtpWebApiProject.Models;

namespace FtpWebApiProject.Services
{
    public interface IFtpService
    {
        // Dosya yükleme metodu tanımı
        Task<bool> UploadFileAsync(IFormFile file, string remotePath);
        
        // Dosya listeleme metodu tanımı (İhtiyacın olabilir)
        Task<IEnumerable<string>> ListFilesAsync(string remotePath);
    }
}