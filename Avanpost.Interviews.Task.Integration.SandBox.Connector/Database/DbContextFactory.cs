using Avanpost.Interviews.Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector.Database
{
    internal class DbContextFactory
    {
        private readonly string connectionString;
        private readonly string provider;
        private readonly string schema;

        private DbContextFactory(string connectionString, string provider, string schema)
        {
            this.connectionString = connectionString;
            this.provider = provider;
            this.schema = schema;
        }

        public static DbContextFactory Create(string? connectionString, string? provider, string? schema)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Не задана строка подключения");

            if (string.IsNullOrEmpty(provider))
                throw new Exception("Не задан провайдер БД");

            if (string.IsNullOrEmpty(schema))
                throw new Exception("Не задана схема БД");

            return new DbContextFactory(connectionString, provider, schema);
        }

        public static DbContextFactory Create(Dictionary<string, string> connectionParams)
        {
            connectionParams.TryGetValue("ConnectionString", out var connectionString);
            connectionParams.TryGetValue("Provider", out var provider);
            connectionParams.TryGetValue("SchemaName", out var schema);

            return Create(connectionString, provider, schema);
        }

        public DbContext GetContext()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<DataContext>();

            if (provider.Contains("SqlServer", StringComparison.InvariantCultureIgnoreCase))
                dbContextOptionsBuilder.UseSqlServer(connectionString);
            else if (provider.Contains("PostgreSQL", StringComparison.InvariantCultureIgnoreCase))
                dbContextOptionsBuilder.UseNpgsql(connectionString);
            else
                throw new Exception("Неопределенный провайдер - " + provider);

            return new DbContext(dbContextOptionsBuilder.Options, schema);
        }
    }
}
