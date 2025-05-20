using AutoMapper;
using ProjectStructure.Share.Attributes;
using ProjectStructure.Share.DTOs.User;
using ProjectStructure.Share.Entities.MsSQL;
using ProjectStructure.Share.Enums;
using ProjectStructure.Share.Interfaces.Repositories;
using ProjectStructure.Share.Interfaces.Serviecs;

namespace ProjectStructure01.Services
{
    /// <summary>
    /// 使用者服務實現
    /// </summary>
    [DI]
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// 初始化使用者服務
        /// </summary>
        /// <param name="unitOfWork">工作單元</param>
        /// <param name="mapper">AutoMapper 映射器</param>
        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 獲取所有使用者
        /// </summary>
        /// <returns>使用者DTO集合</returns>
        public async Task<IEnumerable<OutUserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            if(users == null || !users.Any())
            {
                throw new Exception("沒有使用者");
            }
            return _mapper.Map<IEnumerable<OutUserDto>>(users);
        }

        /// <summary>
        /// 根據ID獲取使用者
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <returns>使用者DTO</returns>
        public async Task<OutUserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(id);
            return _mapper.Map<OutUserDto>(user);
        }

        /// <summary>
        /// 根據用戶名獲取使用者
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>使用者DTO</returns>
        public async Task<OutUserDto?> GetUserByUsernameAsync(string username)
        {
            var users = await _unitOfWork.GetRepository<User>().GetAsync(u => u.Username == username);
            var user = users.FirstOrDefault();
            return _mapper.Map<OutUserDto>(user);
        }

        /// <summary>
        /// 根據電子郵件獲取使用者
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>使用者DTO</returns>
        public async Task<OutUserDto?> GetUserByEmailAsync(string email)
        {
            var users = await _unitOfWork.GetRepository<User>().GetAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            return _mapper.Map<OutUserDto>(user);
        }

        /// <summary>
        /// 創建新使用者
        /// </summary>
        /// <param name="userDto">使用者創建DTO</param>
        /// <returns>創建的使用者DTO</returns>
        public async Task<OutUserDto> CreateUserAsync(InCreateUserDto userDto)
        {
            // 檢查用戶名是否已存在
            if (await IsUsernameExistsAsync(userDto.Username))
            {
                throw new InvalidOperationException($"用戶名 '{userDto.Username}' 已被使用");
            }

            // 檢查電子郵件是否已存在
            if (await IsEmailExistsAsync(userDto.Email))
            {
                throw new InvalidOperationException($"電子郵件 '{userDto.Email}' 已被使用");
            }

            // 映射 DTO 到實體
            var user = _mapper.Map<User>(userDto);

            // 新增使用者
            await _unitOfWork.GetRepository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OutUserDto>(user);
        }

        /// <summary>
        /// 更新使用者
        /// </summary>
        /// <param name="userDto">使用者更新DTO</param>
        /// <returns>更新後的使用者DTO</returns>
        public async Task<OutUserDto> UpdateUserAsync(InUpdateUserDto userDto)
        {
            // 檢查用戶是否存在
            var existingUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(userDto.UserId);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"ID為 '{userDto.UserId}' 的使用者不存在");
            }

            // 檢查用戶名是否被其他用戶使用
            if (!string.IsNullOrEmpty(userDto.Username) && userDto.Username != existingUser.Username)
            {
                var userWithSameUsername = (await _unitOfWork.GetRepository<User>().GetAsync(u => u.Username == userDto.Username && u.UserId != userDto.UserId)).FirstOrDefault();
                if (userWithSameUsername != null)
                {
                    throw new InvalidOperationException($"用戶名 '{userDto.Username}' 已被其他用戶使用");
                }
            }

            // 檢查電子郵件是否被其他用戶使用
            if (!string.IsNullOrEmpty(userDto.Email) && userDto.Email != existingUser.Email)
            {
                var userWithSameEmail = (await _unitOfWork.GetRepository<User>().GetAsync(u => u.Email == userDto.Email && u.UserId != userDto.UserId)).FirstOrDefault();
                if (userWithSameEmail != null)
                {
                    throw new InvalidOperationException($"電子郵件 '{userDto.Email}' 已被其他用戶使用");
                }
            }

            // 映射 DTO 到實體，只更新非空屬性
            _mapper.Map(userDto, existingUser);

            // 更新使用者
            await _unitOfWork.GetRepository<User>().UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OutUserDto>(existingUser);
        }

        /// <summary>
        /// 刪除使用者
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            // 檢查用戶是否存在
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // 刪除使用者
            bool result = await _unitOfWork.GetRepository<User>().DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return result;
        }

        /// <summary>
        /// 檢查使用者名稱是否存在
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _unitOfWork.GetRepository<User>().ExistsAsync(u => u.Username == username);
        }

        /// <summary>
        /// 檢查電子郵件是否存在
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _unitOfWork.GetRepository<User>().ExistsAsync(u => u.Email == email);
        }

        #region Dapper

        /// <summary>
        /// 使用Dapper獲取所有使用者
        /// </summary>
        /// <returns>使用者DTO集合</returns>
        public async Task<IEnumerable<OutUserDto>> GetAllUsersDapperAsync()
        {
            var users = await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).GetAllAsync();
            if (users == null || !users.Any())
            {
                throw new Exception("沒有使用者");
            }
            return _mapper.Map<IEnumerable<OutUserDto>>(users);
        }

        /// <summary>
        /// 使用Dapper根據ID獲取使用者
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <returns>使用者DTO</returns>
        public async Task<OutUserDto?> GetUserByIdDapperAsync(Guid id)
        {
            var user = await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).GetByIdAsync(id);
            return _mapper.Map<OutUserDto>(user);
        }

        /// <summary>
        /// 使用Dapper根據用戶名獲取使用者
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>使用者DTO</returns>
        public async Task<OutUserDto?> GetUserByUsernameDapperAsync(string username)
        {
            var users = await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).GetAsync(u => u.Username == username);
            var user = users.FirstOrDefault();
            return _mapper.Map<OutUserDto>(user);
        }

        /// <summary>
        /// 使用Dapper根據電子郵件獲取使用者
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>使用者DTO</returns>
        public async Task<OutUserDto?> GetUserByEmailDapperAsync(string email)
        {
            var users = await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).GetAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            return _mapper.Map<OutUserDto>(user);
        }

        /// <summary>
        /// 使用Dapper創建新使用者
        /// </summary>
        /// <param name="userDto">使用者創建DTO</param>
        /// <returns>創建的使用者DTO</returns>
        public async Task<OutUserDto> CreateUserDapperAsync(InCreateUserDto userDto)
        {
            // 檢查用戶名是否已存在
            if (await IsUsernameExistsDapperAsync(userDto.Username))
            {
                throw new InvalidOperationException($"用戶名 '{userDto.Username}' 已被使用");
            }

            // 檢查電子郵件是否已存在
            if (await IsEmailExistsDapperAsync(userDto.Email))
            {
                throw new InvalidOperationException($"電子郵件 '{userDto.Email}' 已被使用");
            }

            // 映射 DTO 到實體
            var user = _mapper.Map<User>(userDto);

            // 使用Dapper新增使用者
            await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OutUserDto>(user);
        }

        /// <summary>
        /// 使用Dapper更新使用者
        /// </summary>
        /// <param name="userDto">使用者更新DTO</param>
        /// <returns>更新後的使用者DTO</returns>
        public async Task<OutUserDto> UpdateUserDapperAsync(InUpdateUserDto userDto)
        {
            // 檢查用戶是否存在
            var existingUser = await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).GetByIdAsync(userDto.UserId);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"ID為 '{userDto.UserId}' 的使用者不存在");
            }

            // 檢查用戶名是否被其他用戶使用
            if (!string.IsNullOrEmpty(userDto.Username) && userDto.Username != existingUser.Username)
            {
                var userWithSameUsername = (await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper)
                    .GetAsync(u => u.Username == userDto.Username && u.UserId != userDto.UserId)).FirstOrDefault();
                if (userWithSameUsername != null)
                {
                    throw new InvalidOperationException($"用戶名 '{userDto.Username}' 已被其他用戶使用");
                }
            }

            // 檢查電子郵件是否被其他用戶使用
            if (!string.IsNullOrEmpty(userDto.Email) && userDto.Email != existingUser.Email)
            {
                var userWithSameEmail = (await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper)
                    .GetAsync(u => u.Email == userDto.Email && u.UserId != userDto.UserId)).FirstOrDefault();
                if (userWithSameEmail != null)
                {
                    throw new InvalidOperationException($"電子郵件 '{userDto.Email}' 已被其他用戶使用");
                }
            }

            // 映射 DTO 到實體，只更新非空屬性
            _mapper.Map(userDto, existingUser);

            // 使用Dapper更新使用者
            await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OutUserDto>(existingUser);
        }

        /// <summary>
        /// 使用Dapper刪除使用者
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> DeleteUserDapperAsync(Guid id)
        {
            // 檢查用戶是否存在
            var user = await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // 使用Dapper刪除使用者
            bool result = await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return result;
        }

        /// <summary>
        /// 使用Dapper檢查使用者名稱是否存在
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsUsernameExistsDapperAsync(string username)
        {
            return await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).ExistsAsync(u => u.Username == username);
        }

        /// <summary>
        /// 使用Dapper檢查電子郵件是否存在
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsEmailExistsDapperAsync(string email)
        {
            return await _unitOfWork.GetRepository<User>(RepositoryImplementationType.Dapper).ExistsAsync(u => u.Email == email);
        }

        #endregion
    }
}