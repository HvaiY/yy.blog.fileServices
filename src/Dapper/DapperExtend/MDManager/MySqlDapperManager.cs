using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Dapper;
using DapperMysqlExtend.Model;

namespace DapperExtend.MDManager
{
    public class MySqlDapperManager : IMySqlDapperManager, IDisposable
    {
        private DbConnection conn;
        public MySqlDapperManager()
        {
            conn = DapperMysqlFactory.GetConn();
        }
        public void Delete<T>(string Id) where T : IEntityKey
        {
            conn.Execute($"delete from {typeof(T).Name } where id=@id ", Id);
        }

        public T Get<T, WT>(WT wt)
        {
            var props = typeof(WT).GetProperties();
            var whereStr = string.Join(" and ", props.Select(p => $"{p.Name}=@{p.Name}"));
            var sql = $"select * from {typeof(T).Name}   where {whereStr}";
            var result = conn.Query<T>(sql, wt).FirstOrDefault();
            return result;
        }

        public bool Insert<T>(T t)
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.CustomAttributes.All(at => !at.AttributeType.Equals(typeof(IgonreAttribute)))).Select(p => p.Name);
            var propStr = string.Join(",", props);
            var propParams = string.Join(",", props.Select(p => $"@{p}"));
            var sql = $"insert {typeof(T).Name}({propStr}) values({propParams}) ";
            var isOk = conn.Execute(sql, t) > 0;
            return isOk;
        }



        public List<T> Query<T, WT>(WT wt)
        {
            var props = typeof(WT).GetProperties();
            var whereStr = string.Join(" and ", props.Select(p => $"{p.Name}=@{p.Name}"));
            var sql = $"select * from {typeof(T).Name} as   where {whereStr}";
            return conn.Query<T>(sql, wt).ToList();
        }

        public T QueryLimitFirst<T>(string fileFingerPrint)
        {
            //var sql = $"select * from {typeof(T).Name} as t where t.{nameof(fileFingerPrint)}=@{nameof(fileFingerPrint)} limit 1";
            var sql = $"select * from {typeof(T).Name} as t where t.{nameof(fileFingerPrint)}=@{nameof(fileFingerPrint)} limit 1";
            return conn.Query<T>(sql, fileFingerPrint).FirstOrDefault();

        }

        public void Update<T>(T setValue) where T : IEntityKey
        {
            var props = typeof(T).GetProperties();
            var setStr = string.Join(",", props.Select(p => $"{p.Name}=@{p.Name}"));
            var sql = $"update {typeof(T).Name }{setStr} where id=@id";
            conn.Execute(sql, setValue);
        }
        public T Get<T>(Func<string, (string, object)> fc)
        {
            var sql = $"select * from {typeof(T).Name}   where 1=1 ";
            var (sqlWhere, value) = fc(sql);
            var result = conn.Query<T>(sqlWhere, value).FirstOrDefault();
            return result;
        }

        public void Dispose()
        {
            ConnDispose();
        }
        private void ConnDispose()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }


    }
}
