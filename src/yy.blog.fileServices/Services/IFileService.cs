using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yy.blog.file.Modals;
using yy.blog.fileServices.Services.Dto;

namespace yy.blog.file.Services
{
    public interface IFileService
    {
        Task<Guid> UploadAsync(IFormFile file);
        (FileInfos,FileBusiness) GetFileById(Guid id);
        Task<Guid> UploadBase64StrAsync(Base64StrInfo file);
        List<FileInfoWithBase> GetAll();
    }
}
