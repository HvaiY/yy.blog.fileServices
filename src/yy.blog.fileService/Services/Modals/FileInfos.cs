using DapperMysqlExtend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yy.blog.file.Modals
{
    /// <summary>
    ///  文件信息
    /// </summary>
    public class FileInfos
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 指纹  
        /// </summary>
        public string FileFingerPrint { get; set; }
        public string FileUrl { get; set; }
        public int FileType { get; set; }
        public string FileSuffix { get; set; }
        public string Remark { get; set; }
        //[Igonre]
        //public List<FileBusiness> FileBusiness { get; set; }

        public DateTime? CreateTime { get; set; }
        public DateTime? ModifyTime { get; set; }
        public bool IsDelete { get; set; }
    }

    public class QueryWhereEntity : IEntityKey
    {
        public Guid Id { get; set; }

    }
    public class GetFingerPrintWhere
    {
        public string FileFingerPrint { get; set; }
    }

}
