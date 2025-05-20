using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using ProjectStructure.Share.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectStructure01
{
    /// <summary>
    /// Swagger配置選項類，用於設定API版本文檔
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions> // 實現IConfigureNamedOptions接口，用於配置Swagger選項
    {
        private readonly IApiVersionDescriptionProvider _provider; // 宣告API版本描述提供者
        private readonly IWebHostEnvironment _environment; // 宣告環境變數提供者

        /// <summary>
        /// 構造函數，注入API版本描述提供者
        /// </summary>
        /// <param name="provider">API版本描述提供者</param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IWebHostEnvironment environment) // 通過依賴注入獲取API版本描述提供者
        {
            _provider = provider; // 將注入的提供者賦值給私有成員
            _environment = environment;
        }

        /// <summary>
        /// 配置Swagger生成選項
        /// </summary>
        /// <param name="options">Swagger生成選項</param>
        public void Configure(SwaggerGenOptions options) // 實現Configure方法，用於配置Swagger選項
        {
            // 為每個版本添加一個SwaggerDoc
            foreach (var description in _provider.ApiVersionDescriptions) // 遍歷所有API版本描述
            {
                options.SwaggerDoc( // 為每個版本添加文檔配置
                    description.GroupName, // 使用版本組名作為文檔名
                    CreateVersionInfo(description)); // 創建該版本的信息對象
            }
        }

        /// <summary>
        /// 命名配置方法，用於配置特定名稱的Swagger選項
        /// </summary>
        /// <param name="name">配置名稱</param>
        /// <param name="options">Swagger生成選項</param>
        public void Configure(string name, SwaggerGenOptions options) // 實現帶有名稱參數的Configure方法
        {
            Configure(options); // 直接調用無參的Configure方法
        }

        /// <summary>
        /// 創建API版本信息
        /// </summary>
        /// <param name="description">API版本描述</param>
        /// <returns>OpenAPI信息對象</returns>
        private OpenApiInfo CreateVersionInfo(ApiVersionDescription description) // 私有方法，用於創建版本信息
        {
            var info = new OpenApiInfo() // 創建新的OpenAPI信息對象
            {
                Title = $"ProjectStructure API - {_environment.EnvironmentName}", // 設置API標題包含環境名稱
                Version = description.ApiVersion.ToString(), // 設置版本號
                Description = "ProjectStructure API with versioning." // 設置描述信息
            };

            if (description.IsDeprecated) // 判斷該版本是否已棄用
            {
                info.Description += " This API version has been deprecated."; // 如果已棄用，在描述中添加提示
            }

            return info; // 返回創建的信息對象
        }
    }
}
