using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using yy.blog.file.Services;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System;
using System.ComponentModel.DataAnnotations;
using yy.blog.fileServices.Services.Dto;

namespace yy.blog.file.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {

        private readonly IFileService fileService;

        public FileController(IFileService fileService)
        {
            this.fileService = fileService;
        }

        /// <summary>
        ///  获取文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("id")]
        [ProducesResponseType(typeof(FileResult), 200)]
        public IActionResult GetFileById([FromQuery] [Required]Guid id)
        {
            var (fileInfo, fileBusiness) = fileService.GetFileById(id);
            var fileUrl = fileInfo.FileUrl;
            var fileStream = new FileStream(fileUrl, FileMode.Open);
            var actionresult = new FileStreamResult(fileStream, "application/octet-stream");
            actionresult.FileDownloadName = fileBusiness.Name;
            return actionresult;

        }

        /// <summary>
        ///  上传文件
        /// </summary>
        /// <remarks>
        /// 大文件上传 限制修改 
        /// 1、action 配置特性 RequestFormLimits、RequestSizeLimit
        /// 2、Program 配置 Kestrel  MaxRequestBodySize 
        /// 3、services 配置 MultipartBodyLengthLimit  FormOptions
        /// 4、注意 IIS调试的情况下暂时未知如何处理
        /// </remarks>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> AddFile([Required] IFormFile file)
        {
            var id = await fileService.UploadAsync(file);
            return Ok(new
            {
                message = "成功上传",
                payload = new { FileId = id }
            });
        }

        /// <summary>
        ///  base64 上传为图片保存
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("img")]  
        public async Task<IActionResult> AddBase64Str([Required] Base64StrInfo file)
        { 
            var id = await fileService.UploadBase64StrAsync(file);
            return Ok(new {
                message = "成功上传",
                payload = new { FileId = id }
            });
        }
    }
}
