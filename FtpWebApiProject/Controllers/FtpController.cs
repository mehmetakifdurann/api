using FtpWebApiProject.Services;
using FtpWebApiProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace FtpWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FtpController : ControllerBase
    {
        private readonly IFtpService _ftpService;

        public FtpController(IFtpService ftpService)
        {
            _ftpService = ftpService;
        }

        // 1. PING
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "Pong! API sunucusu aktif ve çalışıyor." });
        }

        // 1.5 (İSTEĞE BAĞLI): FTP debug
        [HttpGet("debug-ftp")]
        public async Task<IActionResult> DebugFtp()
        {
            try
            {
                // Sadece FTP'ye bağlanmayı dener
                var files = await _ftpService.ListFilesAsync("/");
                return Ok(new { message = "FTP bağlantısı başarılı.", count = files.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"FTP bağlantı hatası: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        // 2. UPLOAD
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadModel model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("Lütfen geçerli bir dosya seçin.");

            string targetFolder = string.IsNullOrEmpty(model.TargetPath) ? "/" : model.TargetPath;

            var result = await _ftpService.UploadFileAsync(model.File, targetFolder);

            if (result)
                return Ok(new { message = "Dosya başarıyla yüklendi." });

            return StatusCode(500, "Dosya yüklenirken FTP hatası oluştu.");
        }

        // 3. LIST
        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                var files = await _ftpService.ListFilesAsync("/");
                return Ok(files);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Liste Hatası: {ex.Message} {ex.InnerException?.Message}");
                return StatusCode(
                    500,
                    $"Listeleme hatası: {ex.Message} {ex.InnerException?.Message}"
                );
            }
        }

        // 4. DOWNLOAD
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("Dosya adı boş olamaz.");

            try
            {
                var fileBytes = await _ftpService.DownloadFileAsync(fileName);

                if (fileBytes == null || fileBytes.Length == 0)
                    return NotFound("Dosya sunucuda bulunamadı veya okunamadı.");

                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İndirme Hatası: {ex.Message}");
                return StatusCode(500, "İndirme sırasında sunucu hatası oluştu.");
            }
        }
    }
}