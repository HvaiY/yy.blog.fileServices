using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yy.blog.file.Modals;

namespace yy.blog.file.Services
{
    public interface IFileService
    {
        Task<Guid> UploadAsync(IFormFile file);
        (FileInfos,FileBusiness) GetFileById(Guid id);
    }
}
