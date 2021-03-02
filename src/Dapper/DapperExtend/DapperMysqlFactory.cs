using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.DependencyInjection;
using DapperExtend.MDManager;

 namespace DapperExtend
{
    public class DapperMysqlFactory
    {
        private static DbConnection _conn;
        private DapperMysqlFactory() { }
        private static string _connectionString;

        public static void Init(DapperMysqlOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new ArgumentNullException("ConnectionString should not be empty");
            }
            _connectionString = options.ConnectionString;
        }

        public static DbConnection GetConn() {
            if (_conn == null)
            {

                var factory = MySqlClientFactory.Instance;
                _conn = factory.CreateConnection();
                _conn.ConnectionString = _connectionString;
                _conn.Open();
                if (_conn.State != ConnectionState.Open) throw new InvalidOperationException("should be open!");
            }
            return _conn;
        }
    }

    public class DapperMysqlOptions
    {
        public string ConnectionString { get; set; }
        // TODO More option args
    }

    public static class DapperMysqlDIExtend
    {
        public static void AddDapperMysqlDI(this IServiceCollection services, Action<DapperMysqlOptions> options)
        {
            if (options == null)
            {
                throw new Exception($"Need config connectionString");
            }
            var config = new DapperMysqlOptions();
            options(config);
            DapperMysqlFactory.Init(config);
            services.AddScoped<IMySqlDapperManager, MySqlDapperManager>();
        }
    }

}
