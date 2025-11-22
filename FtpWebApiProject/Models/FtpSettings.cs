namespace FtpWebApiProject.Models
{
    public class FtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; } = 21;
    }
}