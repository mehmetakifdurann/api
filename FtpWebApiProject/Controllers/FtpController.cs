using FtpWebApiProject.Models;
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
                return BadRequest("Lütfen geçerli bir dosya seçin.");

            // Eğer hedef klasör belirtilmezse ana dizine atar
            string targetFolder = string.IsNullOrEmpty(model.TargetPath) ? "/" : model.TargetPath;

            var result = await _ftpService.UploadFileAsync(model.File, targetFolder);

            if (result)
            {
                return Ok(new { message = "Dosya başarıyla FTP'ye yüklendi." });
            }
            else
            {
                return StatusCode(500, "Dosya yüklenirken bir hata oluştu.");
            }
        }
        
        // GET api/ftp/list
        [HttpGet("list")]
        public async Task<IActionResult> ListFiles([FromQuery] string folder = "/")
        {
             var files = await _ftpService.ListFilesAsync(folder);
             return Ok(files);
        }
    }
}