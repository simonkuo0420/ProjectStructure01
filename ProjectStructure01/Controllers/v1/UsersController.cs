using Microsoft.AspNetCore.Mvc;
using ProjectStructure.Share.DTOs.User;
using ProjectStructure.Share.Interfaces.Serviecs;
using ProjectStructure.Share.Extensions;
using System.Net;
using Asp.Versioning;

namespace ProjectStructure01.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// 獲取所有使用者
        /// </summary>
        /// <returns>使用者列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return this.ApiSuccess(users);
        }

        /// <summary>
        /// 根據ID獲取使用者
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <returns>使用者資訊</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return this.ApiError("使用者不存在", HttpStatusCode.NotFound);
            }
            return this.ApiSuccess(user);
        }

        /// <summary>
        /// 創建新使用者
        /// </summary>
        /// <param name="userDto">使用者創建DTO</param>
        /// <returns>創建的使用者資訊</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] InCreateUserDto userDto)
        {
            var createdUser = await _userService.CreateUserAsync(userDto);
            return this.ApiSuccess(createdUser, HttpStatusCode.Created);

        }

        /// <summary>
        /// 更新使用者資訊
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <param name="userDto">使用者更新DTO</param>
        /// <returns>更新後的使用者資訊</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] InUpdateUserDto userDto)
        {
            var updatedUser = await _userService.UpdateUserAsync(userDto);
            return this.ApiSuccess(updatedUser);
        }

        /// <summary>
        /// 刪除使用者
        /// </summary>
        /// <param name="id">使用者ID</param>
        /// <returns>刪除結果</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return this.ApiError("使用者不存在", HttpStatusCode.NotFound);
            }
            return this.ApiSuccess("使用者已成功刪除");
        }
    }
}
