using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yy.blog.fileServices.Services.Dto
{
    public class FileInfoWithBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FileFingerPrint { get; set; }
        public string FileUrl { get; set; }
    }
}
