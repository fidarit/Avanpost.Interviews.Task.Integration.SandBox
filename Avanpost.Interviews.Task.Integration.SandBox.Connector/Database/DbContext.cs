using Avanpost.Interviews.Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector.Database
{
    internal class DbContext : DataContext
    {
        private readonly string defaultSchema;

        public DbContext(DbContextOptions<DataContext> options, string defaultSchema) : base(options)
        {
            this.defaultSchema = defaultSchema;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(defaultSchema);
        }
    }
}
