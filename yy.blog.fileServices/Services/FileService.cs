using DapperExtend.MDManager;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yy.blog.file.Modals;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using System.Text;

namespace yy.blog.file.Services
{
    public class FileService : IFileService
    {
        private readonly IMySqlDapperManager mySqlDapperManager;
        private readonly IWebHostEnvironment webHost;

        public FileService(IMySqlDapperManager mySqlDapperManager, IWebHostEnvironment webHost)
        {
            this.mySqlDapperManager = mySqlDapperManager;
            this.webHost = webHost;
        }
        public (FileInfos, FileBusiness) GetFileById(Guid id)
        {

            var fileBusiness = mySqlDapperManager.Get<FileBusiness, QueryWhereEntity>(new QueryWhereEntity { Id = id });
            if (fileBusiness == null)
            {

                throw new Exception("file not found ");
            }
            var fileInfo = mySqlDapperManager.Get<FileInfos, QueryWhereEntity>(new QueryWhereEntity { Id = fileBusiness.FileInfoId });

            return (fileInfo, fileBusiness);
        }

        public async Task<Guid> UploadAsync(IFormFile file)
        {
            //临时本地存储
            var filePath = await SaveFile(file);
            // 文件指纹获取
            var fileFingerPrint = GetFingerPrint(filePath);
            //var fileInfo = mySqlDapperManager.Get<FileInfos, GetFingerPrintWhere>(new GetFingerPrintWhere {FileFingerPrint= fileFingerPrint });
            // 条件拼接获取 匿名对象无需定义model传值
            var fileInfo = mySqlDapperManager.Get<FileInfos>(sql =>
            {
                var temp = new { FileFingerPrint = fileFingerPrint };
                var props = temp.GetType().GetProperties();
                var whereStr = string.Join(" and ", props.Select(p => $"{p.Name}=@{p.Name}"));
                var sqlWhere = $"{sql} and {whereStr}";
                return (sqlWhere, temp);
            });
            var fileBusiness = new FileBusiness();
            fileBusiness.Name = file.FileName;
            fileBusiness.BusinessName = null;
            fileBusiness.Remark = null;
            if (fileInfo != null)
            {
                File.Delete(filePath);
            }
            else
            {
                var fileName = $"files/{file.FileName}";
                var fileTargetPath = $"{ Path.Combine(webHost.WebRootPath, fileName)}";
                if (File.Exists(fileTargetPath))
                {
                    File.Delete(fileTargetPath);
                }
                File.Move(filePath, fileTargetPath);
                fileInfo = new FileInfos
                {
                    Id = Guid.NewGuid(),
                    FileFingerPrint = fileFingerPrint,
                    FileType = (int)FileTypes.Other,
                    FileUrl = fileTargetPath,
                    FileSuffix = Path.GetExtension(fileTargetPath),
                    Remark = null

                };
                mySqlDapperManager.Insert(fileInfo);
            }
            fileBusiness.FileInfoId = fileInfo.Id;
            // 是否存在相同的文件 指纹相同 名称相同 -- 业务相同(暂时不考虑) 
            var filebusinessTemp = mySqlDapperManager.Get<FileBusiness>(sql =>
            {
                var temp = new { FileInfoId = fileBusiness.FileInfoId,Name= fileBusiness.Name };
                var props = temp.GetType().GetProperties();
                var whereStr = string.Join(" and ", props.Select(p => $"{p.Name}=@{p.Name}"));
                var sqlWhere = $"{sql} and {whereStr}";
                return (sqlWhere, temp);
            });
            if (filebusinessTemp != null) {
                return filebusinessTemp.Id;
            } 
            fileBusiness.Id = Guid.NewGuid();
            if (!mySqlDapperManager.Insert(fileBusiness))
            {
                throw new Exception("insert's failed");
            }
            return fileBusiness.Id;
        }

        /// <summary>
        ///  获取文件指纹
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetFingerPrint(string filePath)
        {
            var md5 = MD5.Create();
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var data = md5.ComputeHash(fileStream);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString().ToLower();
            }
        }

        /// <summary>
        /// 保存文件到临时文件目录
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<string> SaveFile(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                var fileName = $"tempFiles/{file.FileName}";
                var filePath = $"{ Path.Combine(webHost.WebRootPath, fileName)}";
                if (File.Exists(filePath))
                {
                    return filePath;
                }
                var len = 0;
                var buffer = new byte[1024 * 1024 * 5];
                using (var writeStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    while ((len = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await writeStream.WriteAsync(buffer, 0, len);
                    }
                }
                return filePath;
            }
        }
    }
}
