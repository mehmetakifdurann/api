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

        // POST api/ftp/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadModel model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            var result = await _ftpService.UploadFileAsync(model.File, "/");
            if (result) return Ok(new { message = "Dosya yüklendi." });
            return StatusCode(500, "Yükleme hatası.");
        }

        // GET api/ftp/list
        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
             var files = await _ftpService.ListFilesAsync("/");
             return Ok(files);
        }

        // YENİ EKLENEN KISIM: İNDİRME
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string fileName)
        {
            var fileBytes = await _ftpService.DownloadFileAsync(fileName);
            if (fileBytes == null) return NotFound("Dosya bulunamadı.");
            
            // Dosyayı kullanıcıya gönder
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}