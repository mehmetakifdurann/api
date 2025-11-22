using FtpWebApiProject.Services;
using Microsoft.AspNetCore.Mvc;
using FtpWebApiProject.Models; // BU SATIR EKLENDİ (Hata Çözücü)

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

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest("Dosya adı boş olamaz");

            var fileBytes = await _ftpService.DownloadFileAsync(fileName);
            if (fileBytes == null) return NotFound("Dosya sunucuda bulunamadı.");
            
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}