namespace FtpWebApiProject.Models
{
    public class FileUploadModel
    {
        public IFormFile File { get; set; }
        public string TargetPath { get; set; } = "/";
    }
}