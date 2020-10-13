using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace SqlServerHealthModule
{
    public class SqlHealthService
    {
        private readonly string connectionString;

        public SqlHealthService(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public async Task DoHealthCheckAsync()
        {
            AsyncRetryPolicy policy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

            await policy.ExecuteAsync(async () =>
            {
                using (var connection = new SqlConnection(this.connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SELECT @@version", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            });
        }
    }
}
