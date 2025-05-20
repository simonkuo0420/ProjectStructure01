using System.Linq.Expressions;

namespace ProjectStructure.Share.Interfaces.Repositories
{
    /// <summary>
    /// 定義通用儲存庫的基本操作介面
    /// 此介面應該放在 Interfaces 資料夾中
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 獲取所有實體
        /// </summary>
        /// <returns>實體集合</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// 根據條件查詢實體
        /// </summary>
        /// <param name="filter">查詢條件</param>
        /// <returns>符合條件的實體集合</returns>
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// 根據ID獲取實體
        /// </summary>
        /// <param name="id">實體ID</param>
        /// <returns>實體對象，如果未找到則為null</returns>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// 新增實體
        /// </summary>
        /// <param name="entity">要新增的實體</param>
        /// <returns>新增後的實體</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// 批量新增實體
        /// </summary>
        /// <param name="entities">要新增的實體集合</param>
        /// <returns>新增後的實體集合</returns>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// 更新實體
        /// </summary>
        /// <param name="entity">要更新的實體</param>
        /// <returns>已更新的實體</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// 批量更新實體
        /// </summary>
        /// <param name="entities">要更新的實體集合</param>
        /// <returns>已更新的實體集合</returns>
        Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// 刪除實體
        /// </summary>
        /// <param name="entity">要刪除的實體</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// 根據ID刪除實體
        /// </summary>
        /// <param name="id">實體ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeleteByIdAsync(Guid id);

        /// <summary>
        /// 批量刪除實體
        /// </summary>
        /// <param name="entities">要刪除的實體集合</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// 檢查是否存在符合條件的實體
        /// </summary>
        /// <param name="filter">查詢條件</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
    }
}
