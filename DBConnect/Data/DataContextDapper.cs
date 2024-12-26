using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace DBConnect.Data
{
    public class DataContextDapper{
        // private IConfiguration _configuration;
        private string _connectionString;

        public DataContextDapper(IConfiguration configuration){
            // _configuration = configuration;
            _connectionString = configuration?.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
        }

        public IEnumerable<T> LoadData<T>(string sql){
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Query<T>(sql);
        }

        public T LoadDataSingle<T>(string sql){
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.QuerySingle<T>(sql);
        }

        public bool SaveData<T>(string sql, T data){
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Execute(sql, data)>0;
        }

        public int SaveDataWithRowCount<T>(string sql, T data){
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Execute(sql, data);
        }


    }
}