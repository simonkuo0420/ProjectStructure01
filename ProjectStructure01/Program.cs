using Autofac.Extensions.DependencyInjection;
using Autofac;
using ProjectStructure.Share.Attributes;
using System.Reflection;
using ProjectStructure.Share.Configs;
using Microsoft.EntityFrameworkCore;
using ProjectStructure.Share.Filter;
using ProjectStructure.Share.Entities.MsSQL;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Formatting.Json;
using Asp.Versioning;
using ProjectStructure01;
using Asp.Versioning.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// appsetting 環境設定
string envName = builder.Environment.EnvironmentName;
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 配置 Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    // 基本配置
    configuration
        .MinimumLevel.Information()  // 設置最低日誌級別
        .Enrich.FromLogContext()     // 增強日誌內容，添加來自 LogContext 的屬性
        .Enrich.WithProperty("Application", "ProjectStructure01")  // 添加固定屬性
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

    // 根據環境條件添加控制台日誌
    if (context.HostingEnvironment.IsDevelopment())
    {
        configuration.WriteTo.Console();
    }

    // 無論環境如何，都添加 Loki 接收器
    configuration.WriteTo.GrafanaLoki(
        "http://localhost:3100",   // Loki 服務的 URL 地址
        labels: new[] {
            new LokiLabel { Key = "environment", Value = context.HostingEnvironment.EnvironmentName }
        },
        propertiesAsLabels: new[] { "level" },  // 設置哪些屬性作為標籤
        textFormatter: new JsonFormatter()  // 使用 JSON 格式化器
    );
});

// Config 設定檔
builder.Services.Configure<MsSQLConfig>(builder.Configuration.GetSection(MsSQLConfig.Position));
// builder.Services.Configure<PostgresSQLConfig>(builder.Configuration.GetSection(PostgresSQLConfig.Position));

// 註冊DbContext
builder.Services.AddDbContext<MsSQLContext>(options =>
    options.UseSqlServer(builder.Configuration.GetSection(MsSQLConfig.Position)
    .Get<MsSQLConfig>()?.ConnectionString));

//builder.Services.AddDbContext<PostgresSQLContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetSection(PostgresSQLConfig.Position)
//    .Get<PostgresSQLConfig>()?.ConnectionString));

// 註冊 Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()); // Autofac 作為依賴注入容器，替代默認的內建容器
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies()) // 掃描所有已載入到應用域的程序集
    .Where(t => t.GetCustomAttribute<DIAttribute>() != null) // 只註冊那些標記了 DIAttribute 特性的類型
    .AsImplementedInterfaces() // 將這些類型註冊為它們實現的介面接口
    .InstancePerLifetimeScope(); // 設定生命週期範圍，Scoped 生命週期
});

// 註冊 AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers(options =>
{
    //加入攔截例外的過濾器，避免在多個地方重複catch exception
    options.Filters.Add<GlobalExceptionFilter>();
});

// 新增API版本控制服務
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // 設定預設的API版本為1.0
    options.AssumeDefaultVersionWhenUnspecified = true; // 當請求未指定版本時，使用預設版本
    options.ReportApiVersions = true; // 在HTTP標頭中回傳支援的API版本
}).AddApiExplorer(options =>
{
    // 設定版本組名稱格式
    options.GroupNameFormat = "'v'VVV"; // 定義版本在Swagger中的顯示格式，例如 'v1'
    options.SubstituteApiVersionInUrl = true; // 允許在URL中替換版本參數，例如 api/v{version}/controller
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer(); // 添加 API 探索服務
builder.Services.AddSwaggerGen(); //  添加 Swagger 文檔生成服務
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>(); // 添加自訂 Swagger 選項

//加入Cros設定
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .WithExposedHeaders("*");
    });
    options.AddPolicy("Staging", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .WithExposedHeaders("*");
    });
    options.AddPolicy("Production", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .WithExposedHeaders("*");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in apiVersionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
    app.UseCors("Development");
}
else if (app.Environment.IsStaging())
{
    app.UseCors("Staging");
}
else
{
    app.UseCors("Production");
}

// 添加 Serilog 請求日誌記錄中間件
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
