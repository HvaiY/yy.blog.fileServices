using System;
using System.Collections.Generic;
using System.Text;

namespace DapperMysqlExtend.Model
{
   public  interface IEntityKey
    {
         Guid Id { get; set; }
    }
}
