using ProjectStructure.Share.Enums;

namespace ProjectStructure.Share.Interfaces.Repositories
{
    /// <summary>
    /// 工作單元介面，用於管理交易和儲存庫
    /// 此介面應該放在 Interfaces 資料夾中
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 獲取指定實體類型的儲存庫（預設使用EF Core）
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <returns>儲存庫</returns>
        IRepository<T> GetRepository<T>() where T : class;

        /// <summary>
        /// 獲取指定實體類型和實作類型的儲存庫
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="implementationType">實作類型</param>
        /// <returns>儲存庫</returns>
        IRepository<T> GetRepository<T>(RepositoryImplementationType implementationType) where T : class;

        /// <summary>
        /// 開始交易
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// 提交交易
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// 回滾交易
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// 保存所有更改
        /// </summary>
        /// <returns>影響的行數</returns>
        Task<int> SaveChangesAsync();
    }
}
