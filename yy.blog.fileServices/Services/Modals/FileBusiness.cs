using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yy.blog.file.Modals
{
    /// <summary>
    ///  具体业务文件信息
    /// </summary>
    public class FileBusiness
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string BusinessName { get; set; }
        public string Remark { get; set; }
        public Guid FileInfoId { get; set; }
    }
}
