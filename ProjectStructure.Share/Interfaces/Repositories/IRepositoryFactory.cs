using ProjectStructure.Share.Enums;

namespace ProjectStructure.Share.Interfaces.Repositories
{
    /// <summary>
    /// 儲存庫工廠介面，用於獲取不同實作的儲存庫
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// 獲取Entity Framework實作的儲存庫
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <returns>EF實作的儲存庫</returns>
        IRepository<T> GetEfRepository<T>() where T : class;

        /// <summary>
        /// 獲取Dapper實作的儲存庫
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <returns>Dapper實作的儲存庫</returns>
        IRepository<T> GetDapperRepository<T>() where T : class;
    }
}
