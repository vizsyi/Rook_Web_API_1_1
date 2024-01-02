using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Rook01_08.Data.Dapper
{
    public class DapperDBContext
    {
        //private readonly IConfiguration _config;
        private readonly string? _connStr;

        public DapperDBContext(IConfiguration config)
        {
            //_config = config;
            this._connStr = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(this._connStr)) throw new NoNullAllowedException("Connection string is null!");
        }

        public async Task<IEnumerable<T>> LoadDataAsync<T>(string sql)
        {
            IDbConnection connection = new SqlConnection(_connStr);
            return await connection.QueryAsync<T>(sql);
        }

        public async Task<IEnumerable<T>> LoadDataWithParametersAsync<T>(string sql, DynamicParameters parameters)
        {
            IDbConnection connection = new SqlConnection(_connStr);
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<T> LoadDataSingleAsync<T>(string sql)
        {
            IDbConnection connection = new SqlConnection(_connStr);
            return await connection.QuerySingleAsync<T>(sql);
        }

        public async Task<T> LoadDataSingleWithParametersAsync<T>(string sql, DynamicParameters parameters)
        {
            IDbConnection connection = new SqlConnection(_connStr);
            return await connection.QuerySingleAsync<T>(sql, parameters);
        }

        //public bool ExecuteWithParameters(string sql, List<SqlParameter> parameters)
        public async Task<bool> ExecuteWithParametersAsync(string sql, DynamicParameters parameters)
        {
            IDbConnection connection = new SqlConnection(_connStr);
            return (await connection.ExecuteAsync(sql, parameters)) > 0;//todo: ? the importance of the > 0 in case of SP
        }
    }
}
