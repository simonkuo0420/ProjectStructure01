using ProjectStructure.Share.DTOs.User;

namespace ProjectStructure.Share.Interfaces.Serviecs
{
    /// <summary>
    /// 使用者服務介面
    /// 此介面應該放在 Interfaces 資料夾中
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 獲取所有使用者
        /// </summary>
        /// <returns>使用者DTO集合</returns>
        Task<IEnumerable<OutUserDto>> GetAllUsersAsync();

        /// <summary>
        /// 根據ID獲取使用者
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <returns>使用者DTO，如果未找到則為null</returns>
        Task<OutUserDto?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// 根據用戶名獲取使用者
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>使用者DTO，如果未找到則為null</returns>
        Task<OutUserDto?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// 根據電子郵件獲取使用者
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>使用者DTO，如果未找到則為null</returns>
        Task<OutUserDto?> GetUserByEmailAsync(string email);

        /// <summary>
        /// 創建新使用者
        /// </summary>
        /// <param name="userDto">使用者創建DTO</param>
        /// <returns>創建的使用者DTO</returns>
        Task<OutUserDto> CreateUserAsync(InCreateUserDto userDto);

        /// <summary>
        /// 更新使用者
        /// </summary>
        /// <param name="userDto">使用者更新DTO</param>
        /// <returns>更新後的使用者DTO</returns>
        Task<OutUserDto> UpdateUserAsync(InUpdateUserDto userDto);

        /// <summary>
        /// 刪除使用者
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeleteUserAsync(Guid id);

        /// <summary>
        /// 檢查使用者名稱是否存在
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>是否存在</returns>
        Task<bool> IsUsernameExistsAsync(string username);

        /// <summary>
        /// 檢查電子郵件是否存在
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>是否存在</returns>
        Task<bool> IsEmailExistsAsync(string email);
    }
}

