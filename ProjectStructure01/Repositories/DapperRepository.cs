using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ProjectStructure.Share.Attributes;
using ProjectStructure.Share.Configs;
using ProjectStructure.Share.Interfaces.Repositories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ProjectStructure01.Repositories
{
    /// <summary>
    /// 使用 Dapper 實現的泛型儲存庫
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    [DI]
    public class DapperRepository<T> : IRepository<T> where T : class
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly PropertyInfo _idProperty;
        private readonly List<PropertyInfo> _properties;

        /// <summary>
        /// 初始化 Dapper 儲存庫
        /// </summary>
        /// <param name="config">MsSQL 配置</param>
        public DapperRepository(IOptions<MsSQLConfig> config)
        {
            _connectionString = config.Value.ConnectionString;
            _tableName = GetTableName();
            _idProperty = GetIdProperty();
            _properties = GetProperties();
        }

        /// <summary>
        /// 獲取表名
        /// </summary>
        /// <returns>表名</returns>
        private string GetTableName()
        {
            var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
            return tableAttr != null ? tableAttr.Name : typeof(T).Name.ToLower();
        }

        /// <summary>
        /// 獲取主鍵屬性
        /// </summary>
        /// <returns>主鍵屬性信息</returns>
        private PropertyInfo GetIdProperty()
        {
            return typeof(T).GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null)
                ?? typeof(T).GetProperties().FirstOrDefault(p => p.Name.Equals($"{typeof(T).Name}Id", StringComparison.OrdinalIgnoreCase))
                ?? typeof(T).GetProperties().FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 獲取所有映射屬性
        /// </summary>
        /// <returns>屬性列表</returns>
        private List<PropertyInfo> GetProperties()
        {
            return typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null)
                .ToList();
        }

        /// <summary>
        /// 創建資料庫連接
        /// </summary>
        /// <returns>IDbConnection 實例</returns>
        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// 獲取所有實體
        /// </summary>
        /// <returns>實體集合</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<T>($"SELECT * FROM {_tableName}");
        }

        /// <summary>
        /// 根據條件查詢實體
        /// </summary>
        /// <param name="filter">查詢條件</param>
        /// <returns>符合條件的實體集合</returns>
        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter)
        {
            // 嘗試將表達式轉換為 SQL
            if (TryParseExpression(filter, out string whereClause, out DynamicParameters parameters))
            {
                // 如果成功轉換
                using var connection = CreateConnection();
                return await connection.QueryAsync<T>($"SELECT * FROM {_tableName} WHERE {whereClause}", parameters);
            }
            else
            {
                // 如果無法轉換為 SQL，則先獲取所有數據再在記憶體中過濾
                // 注意：這種方式對於大型數據集可能會有性能問題
                var allItems = await GetAllAsync();
                return allItems.AsQueryable().Where(filter).ToList();
            }
        }

        /// <summary>
        /// 嘗試將表達式解析為 SQL 語句
        /// </summary>
        /// <param name="filter">LINQ 表達式</param>
        /// <param name="whereClause">輸出 WHERE 子句</param>
        /// <param name="parameters">SQL 參數</param>
        /// <returns>是否成功解析</returns>
        private bool TryParseExpression(Expression<Func<T, bool>> filter, out string whereClause, out DynamicParameters parameters)
        {
            parameters = new DynamicParameters();
            whereClause = string.Empty;

            // 目前僅支援簡單的相等比較表達式
            if (filter.Body is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Equal)
            {
                if (binaryExpr.Left is MemberExpression memberExpr && memberExpr.Member is PropertyInfo property)
                {
                    // 處理 x => x.PropertyName == value
                    string columnName = property.Name;
                    object value = Expression.Lambda(binaryExpr.Right).Compile().DynamicInvoke();

                    whereClause = $"{columnName} = @{columnName}";
                    parameters.Add($"@{columnName}", value);
                    return true;
                }
            }

            // 如果不是簡單的相等比較，則返回失敗
            return false;
        }

        /// <summary>
        /// 根據ID獲取實體
        /// </summary>
        /// <param name="id">實體ID</param>
        /// <returns>實體對象，如果未找到則為null</returns>
        public async Task<T?> GetByIdAsync(Guid id)
        {
            if (_idProperty == null)
                throw new InvalidOperationException("無法確定實體的主鍵屬性");

            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(
                $"SELECT * FROM {_tableName} WHERE {_idProperty.Name} = @Id",
                new { Id = id });
        }

        /// <summary>
        /// 新增實體
        /// </summary>
        /// <param name="entity">要新增的實體</param>
        /// <returns>新增後的實體</returns>
        public async Task<T> AddAsync(T entity)
        {
            var insertableProps = _properties
                .Where(p => p.Name != _idProperty?.Name || !IsAutoIncrementId(_idProperty))
                .ToList();

            string columns = string.Join(", ", insertableProps.Select(p => p.Name));
            string parameters = string.Join(", ", insertableProps.Select(p => $"@{p.Name}"));

            string insertQuery = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters})";

            // 如果 ID 是自增的，則需要返回新生成的 ID
            if (_idProperty != null && IsAutoIncrementId(_idProperty))
            {
                insertQuery += "; SELECT SCOPE_IDENTITY();";
            }

            using var connection = CreateConnection();
            var dynamicParams = new DynamicParameters();
            foreach (var prop in insertableProps)
            {
                dynamicParams.Add($"@{prop.Name}", prop.GetValue(entity));
            }

            if (_idProperty != null && IsAutoIncrementId(_idProperty))
            {
                // 對於自增 ID，獲取新 ID 並設置到實體
                var newId = await connection.ExecuteScalarAsync<int>(insertQuery, dynamicParams);
                _idProperty.SetValue(entity, Convert.ChangeType(newId, _idProperty.PropertyType));
            }
            else
            {
                // 對於 GUID 類型的 ID，直接執行插入
                await connection.ExecuteAsync(insertQuery, dynamicParams);
            }

            return entity;
        }

        /// <summary>
        /// 判斷屬性是否為自增 ID
        /// </summary>
        /// <param name="property">屬性</param>
        /// <returns>是否為自增 ID</returns>
        private bool IsAutoIncrementId(PropertyInfo property)
        {
            // 在 SQL Server 中，通常是 int 類型的主鍵並且有特性 DatabaseGenerated
            var databaseGenAttr = property.GetCustomAttribute<DatabaseGeneratedAttribute>();
            return (property.PropertyType == typeof(int) || property.PropertyType == typeof(long)) &&
                   (databaseGenAttr?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity);
        }

        /// <summary>
        /// 批量新增實體
        /// </summary>
        /// <param name="entities">要新增的實體集合</param>
        /// <returns>新增後的實體集合</returns>
        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            var result = new List<T>();

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var entity in entities)
                {
                    // 為每個實體添加
                    var insertableProps = _properties
                        .Where(p => p.Name != _idProperty?.Name || !IsAutoIncrementId(_idProperty))
                        .ToList();

                    string columns = string.Join(", ", insertableProps.Select(p => p.Name));
                    string parameters = string.Join(", ", insertableProps.Select(p => $"@{p.Name}"));

                    string insertQuery = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters})";

                    if (_idProperty != null && IsAutoIncrementId(_idProperty))
                    {
                        insertQuery += "; SELECT SCOPE_IDENTITY();";
                    }

                    var dynamicParams = new DynamicParameters();
                    foreach (var prop in insertableProps)
                    {
                        dynamicParams.Add($"@{prop.Name}", prop.GetValue(entity));
                    }

                    if (_idProperty != null && IsAutoIncrementId(_idProperty))
                    {
                        var newId = await connection.ExecuteScalarAsync<int>(insertQuery, dynamicParams, transaction);
                        _idProperty.SetValue(entity, Convert.ChangeType(newId, _idProperty.PropertyType));
                    }
                    else
                    {
                        await connection.ExecuteAsync(insertQuery, dynamicParams, transaction);
                    }

                    result.Add(entity);
                }

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 更新實體
        /// </summary>
        /// <param name="entity">要更新的實體</param>
        /// <returns>已更新的實體</returns>
        public async Task<T> UpdateAsync(T entity)
        {
            if (_idProperty == null)
                throw new InvalidOperationException("無法確定實體的主鍵屬性");

            var updatableProps = _properties
                .Where(p => p.Name != _idProperty.Name)
                .ToList();

            string setClause = string.Join(", ", updatableProps.Select(p => $"{p.Name} = @{p.Name}"));

            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            foreach (var prop in updatableProps)
            {
                parameters.Add($"@{prop.Name}", prop.GetValue(entity));
            }
            parameters.Add($"@{_idProperty.Name}", _idProperty.GetValue(entity));

            await connection.ExecuteAsync(
                $"UPDATE {_tableName} SET {setClause} WHERE {_idProperty.Name} = @{_idProperty.Name}",
                parameters);

            return entity;
        }

        /// <summary>
        /// 批量更新實體
        /// </summary>
        /// <param name="entities">要更新的實體集合</param>
        /// <returns>已更新的實體集合</returns>
        public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
        {
            if (_idProperty == null)
                throw new InvalidOperationException("無法確定實體的主鍵屬性");

            var result = new List<T>();

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var entity in entities)
                {
                    var updatableProps = _properties
                        .Where(p => p.Name != _idProperty.Name)
                        .ToList();

                    string setClause = string.Join(", ", updatableProps.Select(p => $"{p.Name} = @{p.Name}"));

                    var parameters = new DynamicParameters();
                    foreach (var prop in updatableProps)
                    {
                        parameters.Add($"@{prop.Name}", prop.GetValue(entity));
                    }
                    parameters.Add($"@{_idProperty.Name}", _idProperty.GetValue(entity));

                    await connection.ExecuteAsync(
                        $"UPDATE {_tableName} SET {setClause} WHERE {_idProperty.Name} = @{_idProperty.Name}",
                        parameters,
                        transaction);

                    result.Add(entity);
                }

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 刪除實體
        /// </summary>
        /// <param name="entity">要刪除的實體</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> DeleteAsync(T entity)
        {
            if (_idProperty == null)
                throw new InvalidOperationException("無法確定實體的主鍵屬性");

            var id = _idProperty.GetValue(entity);
            if (id == null)
                return false;

            return await DeleteByIdAsync((Guid)id);
        }

        /// <summary>
        /// 根據ID刪除實體
        /// </summary>
        /// <param name="id">實體ID</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            if (_idProperty == null)
                throw new InvalidOperationException("無法確定實體的主鍵屬性");

            using var connection = CreateConnection();
            int affectedRows = await connection.ExecuteAsync(
                $"DELETE FROM {_tableName} WHERE {_idProperty.Name} = @Id",
                new { Id = id });

            return affectedRows > 0;
        }

        /// <summary>
        /// 批量刪除實體
        /// </summary>
        /// <param name="entities">要刪除的實體集合</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
        {
            if (_idProperty == null)
                throw new InvalidOperationException("無法確定實體的主鍵屬性");

            if (!entities.Any())
                return true;

            var ids = entities.Select(e => _idProperty.GetValue(e)).ToList();

            using var connection = CreateConnection();

            // SQL Server 不支援在參數中直接使用 IN 查詢，所以需要特殊處理
            StringBuilder query = new StringBuilder();
            query.Append("DELETE FROM ").Append(_tableName).Append(" WHERE ").Append(_idProperty.Name).Append(" IN (");

            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0) query.Append(",");
                query.Append($"@Id{i}");
            }

            query.Append(")");

            var parameters = new DynamicParameters();
            for (int i = 0; i < ids.Count; i++)
            {
                parameters.Add($"@Id{i}", ids[i]);
            }

            int affectedRows = await connection.ExecuteAsync(query.ToString(), parameters);
            return affectedRows > 0;
        }

        /// <summary>
        /// 檢查是否存在符合條件的實體
        /// </summary>
        /// <param name="filter">查詢條件</param>
        /// <returns>是否存在</returns>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            if (TryParseExpression(filter, out string whereClause, out DynamicParameters parameters))
            {
                using var connection = CreateConnection();
                var count = await connection.ExecuteScalarAsync<int>(
                    $"SELECT COUNT(1) FROM {_tableName} WHERE {whereClause}",
                    parameters);
                return count > 0;
            }
            else
            {
                var items = await GetAsync(filter);
                return items.Any();
            }
        }
    }
}