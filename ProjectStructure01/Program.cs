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
// appsetting ���ҳ]�w
string envName = builder.Environment.EnvironmentName;
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// �t�m Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    // �򥻰t�m
    configuration
        .MinimumLevel.Information()  // �]�m�̧C��x�ŧO
        .Enrich.FromLogContext()     // �W�j��x���e�A�K�[�Ӧ� LogContext ���ݩ�
        .Enrich.WithProperty("Application", "ProjectStructure01")  // �K�[�T�w�ݩ�
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

    // �ھ����ұ���K�[����x��x
    if (context.HostingEnvironment.IsDevelopment())
    {
        configuration.WriteTo.Console();
    }

    // �L�����Ҧp��A���K�[ Loki ������
    configuration.WriteTo.GrafanaLoki(
        "http://localhost:3100",   // Loki �A�Ȫ� URL �a�}
        labels: new[] {
            new LokiLabel { Key = "environment", Value = context.HostingEnvironment.EnvironmentName }
        },
        propertiesAsLabels: new[] { "level" },  // �]�m�����ݩʧ@������
        textFormatter: new JsonFormatter()  // �ϥ� JSON �榡�ƾ�
    );
});

// Config �]�w��
builder.Services.Configure<MsSQLConfig>(builder.Configuration.GetSection(MsSQLConfig.Position));
// builder.Services.Configure<PostgresSQLConfig>(builder.Configuration.GetSection(PostgresSQLConfig.Position));

// ���UDbContext
builder.Services.AddDbContext<MsSQLContext>(options =>
    options.UseSqlServer(builder.Configuration.GetSection(MsSQLConfig.Position)
    .Get<MsSQLConfig>()?.ConnectionString));

//builder.Services.AddDbContext<PostgresSQLContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetSection(PostgresSQLConfig.Position)
//    .Get<PostgresSQLConfig>()?.ConnectionString));

// ���U Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()); // Autofac �@���̿�`�J�e���A���N�q�{�����خe��
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies()) // ���y�Ҧ��w���J�����ΰ쪺�{�Ƕ�
    .Where(t => t.GetCustomAttribute<DIAttribute>() != null) // �u���U���ǼаO�F DIAttribute �S�ʪ�����
    .AsImplementedInterfaces() // �N�o���������U�����̹�{���������f
    .InstancePerLifetimeScope(); // �]�w�ͩR�g���d��AScoped �ͩR�g��
});

// ���U AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers(options =>
{
    //�[�J�d�I�ҥ~���L�o���A�קK�b�h�Ӧa�譫��catch exception
    options.Filters.Add<GlobalExceptionFilter>();
});

// �s�WAPI��������A��
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // �]�w�w�]��API������1.0
    options.AssumeDefaultVersionWhenUnspecified = true; // ��ШD�����w�����ɡA�ϥιw�]����
    options.ReportApiVersions = true; // �bHTTP���Y���^�Ǥ䴩��API����
}).AddApiExplorer(options =>
{
    // �]�w�����զW�ٮ榡
    options.GroupNameFormat = "'v'VVV"; // �w�q�����bSwagger������ܮ榡�A�Ҧp 'v1'
    options.SubstituteApiVersionInUrl = true; // ���\�bURL�����������ѼơA�Ҧp api/v{version}/controller
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer(); // �K�[ API �����A��
builder.Services.AddSwaggerGen(); //  �K�[ Swagger ���ɥͦ��A��
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>(); // �K�[�ۭq Swagger �ﶵ

//�[�JCros�]�w
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

// �K�[ Serilog �ШD��x�O��������
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
