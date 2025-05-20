using Microsoft.EntityFrameworkCore;
using ProjectStructure.Share.Attributes;
using ProjectStructure.Share.Entities.MsSQL;
using ProjectStructure.Share.Interfaces.Repositories;
using System.Linq.Expressions;

namespace ProjectStructure01.Repositories
{
    /// <summary>
    /// 使用 Entity Framework Core 實現的泛型儲存庫
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    [DI]
    public class EfRepository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// 資料庫上下文
        /// </summary>
        protected readonly MsSQLContext _context;

        /// <summary>
        /// 初始化儲存庫
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public EfRepository(MsSQLContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 獲取所有實體
        /// </summary>
        /// <returns>實體集合</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        /// <summary>
        /// 根據條件查詢實體
        /// </summary>
        /// <param name="filter">查詢條件</param>
        /// <returns>符合條件的實體集合</returns>
        public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await _context.Set<T>().Where(filter).ToListAsync();
        }

        /// <summary>
        /// 根據ID獲取實體
        /// </summary>
        /// <param name="id">實體ID</param>
        /// <returns>實體對象，如果未找到則為null</returns>
        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// 新增實體
        /// </summary>
        /// <param name="entity">要新增的實體</param>
        /// <returns>新增後的實體</returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        /// <summary>
        /// 批量新增實體
        /// </summary>
        /// <param name="entities">要新增的實體集合</param>
        /// <returns>新增後的實體集合</returns>
        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            return entities;
        }

        /// <summary>
        /// 更新實體
        /// </summary>
        /// <param name="entity">要更新的實體</param>
        /// <returns>已更新的實體</returns>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            // 確保實體被追蹤
            _context.Entry(entity).State = EntityState.Modified;
            return await Task.FromResult(entity);
        }

        /// <summary>
        /// 批量更新實體
        /// </summary>
        /// <param name="entities">要更新的實體集合</param>
        /// <returns>已更新的實體集合</returns>
        public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
            return await Task.FromResult(entities);
        }

        /// <summary>
        /// 刪除實體
        /// </summary>
        /// <param name="entity">要刪除的實體</param>
        /// <returns>操作是否成功</returns>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            return await Task.FromResult(true);
        }

        /// <summary>
        /// 根據ID刪除實體
        /// </summary>
        /// <param name="id">實體ID</param>
        /// <returns>操作是否成功</returns>
        public virtual async Task<bool> DeleteByIdAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return false;
            }
            return await DeleteAsync(entity);
        }

        /// <summary>
        /// 批量刪除實體
        /// </summary>
        /// <param name="entities">要刪除的實體集合</param>
        /// <returns>操作是否成功</returns>
        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
            return await Task.FromResult(true);
        }

        /// <summary>
        /// 檢查是否存在符合條件的實體
        /// </summary>
        /// <param name="filter">查詢條件</param>
        /// <returns>是否存在</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            return await _context.Set<T>().AnyAsync(filter);
        }
    }
}