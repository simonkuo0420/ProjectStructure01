using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using ProjectStructure.Share.Configs;
using ProjectStructure.Share.Entities.MsSQL;

namespace ProjectStructure01
{
    public class MsSQLContextFactory : IDesignTimeDbContextFactory<MsSQLContext>
    {
        public MsSQLContext CreateDbContext(string[] args)
        {
            // 從 appsettings.json 取得連線字串
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var msSQLConfig = new MsSQLConfig();
            configuration.GetSection(MsSQLConfig.Position).Bind(msSQLConfig);

            var optionsBuilder = new DbContextOptionsBuilder<MsSQLContext>();
            optionsBuilder.UseSqlServer(msSQLConfig.ConnectionString);

            return new MsSQLContext(optionsBuilder.Options);
        }
    }
}
