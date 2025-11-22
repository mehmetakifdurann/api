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

        // 1. PING: API'nin ve sunucunun çalışıp çalışmadığını test etmek için
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "Pong! API sunucusu aktif ve çalışıyor." });
        }

        // 2. UPLOAD: Dosya Yükleme
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadModel model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("Lütfen geçerli bir dosya seçin.");

            // Hedef klasör (Varsayılan: Ana Dizin /)
            string targetFolder = string.IsNullOrEmpty(model.TargetPath) ? "/" : model.TargetPath;

            var result = await _ftpService.UploadFileAsync(model.File, targetFolder);

            if (result)
            {
                return Ok(new { message = "Dosya başarıyla yüklendi." });
            }
            else
            {
                return StatusCode(500, "Dosya yüklenirken FTP hatası oluştu.");
            }
        }

        // 3. LIST: Dosyaları Listeleme (Boyut ve Tarih ile)
        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                // Ana dizindeki ( / ) dosyaları getir
                var files = await _ftpService.ListFilesAsync("/");
                return Ok(files);
            }
            catch (Exception ex)
            {
                // Hata olursa logla ve kullanıcıya bildir
                Console.WriteLine($"Liste Hatası: {ex.Message}");
                return StatusCode(500, $"Listeleme hatası: {ex.Message}");
            }
        }

        // 4. DOWNLOAD: Dosya İndirme
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

                // Dosyayı tarayıcıya gönder (Otomatik indirme başlar)
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