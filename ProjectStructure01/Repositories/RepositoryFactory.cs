using Autofac;
using Microsoft.Extensions.Options;
using ProjectStructure.Share.Attributes;
using ProjectStructure.Share.Configs;
using ProjectStructure.Share.Entities.MsSQL;
using ProjectStructure.Share.Enums;
using ProjectStructure.Share.Interfaces.Repositories;

namespace ProjectStructure01.Repositories
{
    /// <summary>
    /// 儲存庫工廠，用於創建特定ORM實現的儲存庫
    /// </summary>
    [DI]
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly MsSQLContext _context;
        private readonly IOptions<MsSQLConfig> _config;

        /// <summary>
        /// 初始化儲存庫工廠
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="config">MsSQL配置</param>
        public RepositoryFactory(MsSQLContext context, IOptions<MsSQLConfig> config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 獲取Entity Framework實作的儲存庫
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <returns>EF實作的儲存庫</returns>
        public IRepository<T> GetEfRepository<T>() where T : class
        {
            return new Repositories.EfRepository<T>(_context);
        }

        /// <summary>
        /// 獲取Dapper實作的儲存庫
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <returns>Dapper實作的儲存庫</returns>
        public IRepository<T> GetDapperRepository<T>() where T : class
        {
            return new Repositories.DapperRepository<T>(_config);
        }
    }
}
