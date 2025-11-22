namespace FtpWebApiProject.Models
{
    public class FileUploadModel
    {
        // IFormFile, ASP.NET Core'da dosya yüklemek için standarttır
        public required IFormFile File { get; set; }
        
        // Opsiyonel: Dosyanın FTP'de hangi klasöre gideceği
        public string? TargetPath { get; set; } 
    }
}