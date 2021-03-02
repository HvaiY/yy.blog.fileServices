using DapperMysqlExtend.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperExtend.MDManager
{
   public  interface IMySqlDapperManager
    {
        void Update<T>(T setValue) where T :IEntityKey;
        bool Insert<T>(T t);
         void Delete<T>(string Id) where T : IEntityKey;
        T Get<T, WT>(WT wt);
        T Get<T>(Func<string ,(string, object)> fc);
        List<T> Query<T,WT>(WT wt);
        T QueryLimitFirst<T>(string fileFingerPrint);
    }
}
