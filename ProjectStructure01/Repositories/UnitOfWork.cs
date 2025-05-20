using Microsoft.EntityFrameworkCore.Storage;
using ProjectStructure.Share.Attributes;
using ProjectStructure.Share.Entities.MsSQL;
using ProjectStructure.Share.Enums;
using ProjectStructure.Share.Interfaces.Repositories;

namespace ProjectStructure01.Repositories
{
    /// <summary>
    /// 工作單元實現，使用 Entity Framework Core
    /// </summary>
    [DI]
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MsSQLContext _context;
        private readonly IRepositoryFactory _repositoryFactory;
        private IDbContextTransaction _transaction;
        private readonly Dictionary<string, object> _repositories;
        private bool _disposed;

        /// <summary>
        /// 初始化工作單元
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="repositoryFactory">儲存庫工廠</param>
        public UnitOfWork(MsSQLContext context, IRepositoryFactory repositoryFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _repositories = new Dictionary<string, object>();
        }

        /// <summary>
        /// 獲取指定實體類型的儲存庫（預設使用EF Core）
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <returns>儲存庫</returns>
        public IRepository<T> GetRepository<T>() where T : class
        {
            return GetRepository<T>(RepositoryImplementationType.EntityFramework);
        }

        /// <summary>
        /// 獲取指定實體類型和實作類型的儲存庫
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="implementationType">實作類型</param>
        /// <returns>儲存庫</returns>
        public IRepository<T> GetRepository<T>(RepositoryImplementationType implementationType) where T : class
        {
            string key = $"{typeof(T).FullName}_{implementationType}";

            if (!_repositories.ContainsKey(key))
            {
                IRepository<T> repository = implementationType switch
                {
                    RepositoryImplementationType.EntityFramework => _repositoryFactory.GetEfRepository<T>(),
                    RepositoryImplementationType.Dapper => _repositoryFactory.GetDapperRepository<T>(),
                    _ => throw new ArgumentException($"不支持的儲存庫實作類型：{implementationType}")
                };

                _repositories[key] = repository;
            }

            return (IRepository<T>)_repositories[key];
        }

        /// <summary>
        /// 開始交易
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// 提交交易
        /// </summary>
        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 回滾交易
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// 保存所有更改
        /// </summary>
        /// <returns>影響的行數</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        /// <param name="disposing">是否正在釋放</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
