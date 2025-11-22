using FtpWebApiProject.Services;
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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadModel model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("Dosya yok.");

            var result = await _ftpService.UploadFileAsync(model.File, "/");
            if (result) return Ok(new { message = "Yüklendi" });
            return StatusCode(500, "Hata oluştu");
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
             var files = await _ftpService.ListFilesAsync("/");
             return Ok(files);
        }

        // İŞTE SORUNLU OLAN KISIM BURASIYDI
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string fileName)
        {
            Console.WriteLine($"API Download Tetiklendi: {fileName}"); // Render Loglarında bunu göreceğiz

            if (string.IsNullOrEmpty(fileName)) return BadRequest("Dosya adı boş olamaz");

            var fileBytes = await _ftpService.DownloadFileAsync(fileName);
            
            if (fileBytes == null) 
            {
                Console.WriteLine("Service null döndü, 404 atılıyor.");
                return NotFound("Dosya sunucuda bulunamadı.");
            }
            
            // Dosya türünü otomatik algıla
            string contentType = "application/octet-stream"; // Varsayılan
            return File(fileBytes, contentType, fileName);
        }
    }
}