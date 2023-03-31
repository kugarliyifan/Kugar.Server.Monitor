using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Kugar.Core;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Kugar.Core.Log;
using Kugar.Core.Serialization;
using Kugar.Core.Web;
using Kugar.Core.Web.Authentications;
using Kugar.Core.Web.JsonTemplate;
using Kugar.Server.MonitorServer.Data;
using Microsoft.AspNetCore.Hosting;
using NSwag.Generation.Processors.Security;
using NSwag;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Serialization;
using Kugar.Core.Services;
using Kugar.Server.MonitorServer.Areas.Dashboard.Helpers;
using Kugar.Server.MonitorServer.Areas.API.Helpers;
using InfluxDB.Client;
using Kugar.Server.MonitorServer.Services;
using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.SignalR;

namespace Kugar.Server.MonitorServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());

            builder.Host.ConfigureAppConfiguration((builderContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); // 配置可热重载
            });

            builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                var baseServiceType = typeof(BaseService);
                builder.RegisterTypes(baseServiceType.Assembly.GetTypes().Where(t => t.IsSubclassOf(baseServiceType) && !t.IsAbstract).ToArrayEx())
                    .InstancePerLifetimeScope()
                    //.EnableClassInterceptors()
                    .PropertiesAutowired(wiringFlags: PropertyWiringOptions.AllowCircularDependencies);

            });

            LoggerManager.LoggerFactory = new NLogFactory();

            builder.Services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(x =>
                new NLoggerProviderForDI.NLoggerForDI((NLogger)LoggerManager.Default));

            builder.Services.AddScoped<InfluxDBClient>((s) =>
                new InfluxDBClient(CustomConfigManager.Default["InfluxDb:Conn"],
                    CustomConfigManager.Default["InfluxDb:Token"]));

            var freesql = new FreeSqlBuilder().UseConnectionString(DataType.SqlServer, CustomConfigManager.Default["Db:Connstr"])
                .UseExitAutoDisposePool(true)
                .UseLazyLoading(true)
                .UseAutoSyncStructure(true) 
                .Build();

            freesql.CodeFirst.SyncStructure(typeof(MonitorDbContext).Assembly.GetTypes().Where(x => !x.IsAbstract && x.GetAttribute<TableAttribute>() != null).ToArray());
            
            builder.Services.AddSingleton<IFreeSql>(freesql);
            builder.Services.AddScoped<MonitorDbContext>();
            builder.Services.AddScoped<UnitOfWorkManager>();

            builder.Services.AddOptions();

            //builder.Services.AddScoped(typeof(IUserPermissionFactoryService), typeof(PermissionCheckFactoryService));

            var showSwagger = CustomConfigManager.Default["ShowSwagger"].ToBool();

            if (showSwagger)
            {
                builder.Services.EnableSyncIO().AddOpenApiDocument(opt =>
                {
                    opt.ApiGroupNames = new[] { "AdminApi" };
                    opt.DocumentName = "Api";
                    opt.Title = "Api接口";

                    opt.AddJsonTemplateV2(typeof(Program).Assembly);
                    opt.UseControllerSummaryAsTagDescription = true;
                    opt.DocumentProcessors.Add(new SecurityDefinitionAppender("Authorization", new OpenApiSecurityScheme()
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "token",

                    }));
                }).AddOpenApiDocument(opt =>
                {
                    opt.ApiGroupNames = new[] { "AppApi" };
                    opt.DocumentName = "AppApi";
                    opt.Title = "Api接口";

                    opt.AddJsonTemplateV2(typeof(Program).Assembly);
                    opt.UseControllerSummaryAsTagDescription = true;
                    opt.DocumentProcessors.Add(new SecurityDefinitionAppender("Authorization", new OpenApiSecurityScheme()
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "token",

                    }));
                });
            }


            builder.Services
             .AddHttpContextAccessor()
             .RegisterGlobalProvider()
             .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
             .AddCors(x =>
                {
                    x.AddDefaultPolicy(x =>
                        x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true));
                })
                 .AddAuthentication()
                 .AddWebJWT("DashboardApi", new WebJWTOption()
                 {
                     LoginService = new DashboardAPILoginService(),
                     TokenEncKey = CustomConfigManager.Default["JWT:Dashboard:Token"],
                     Audience = CustomConfigManager.Default["JWT:Dashboard:Audience"],
                     Issuer = CustomConfigManager.Default["JWT:Dashboard:Issuer"]
                 })
             .AddWebJWT("CollectorApi", new WebJWTOption()
             {
                 LoginService = new ProjectApiLoginService(),
                 TokenEncKey = CustomConfigManager.Default["JWT:Project:Token"],
                 Audience = CustomConfigManager.Default["JWT:Project:Audience"],
                 Issuer = CustomConfigManager.Default["JWT:Project:Issuer"]
             })
             ;


            builder.Services.AddControllers().AddControllersAsServices()
                .AddNewtonsoftJson(
                    (options) =>
                    {
                        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                        (options.SerializerSettings.ContractResolver as DefaultContractResolver).NamingStrategy = new CamelCaseNamingStrategy(true, true);
                        options.SerializerSettings.Converters.Add(new GuidJsonConverter());
                    }
                )
                .EnableJsonValueModelBinder();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (showSwagger)
            {
                app.UseOpenApi();       // serve OpenAPI/Swagger documents

                app.UseSwaggerUi3();    // serve Swagger UI

                app.UseSwaggerUi3(config =>  // serve ReDoc UI
                {
                    config.Path = "/swager";
                    config.WithCredentials = true;
                    config.EnableTryItOut = false;

                });

            }

            app.UseAuthorization()
                .UseJsonTemplate();
            app.UseCookiePolicy();
            

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();
       

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
            });

            app.MapControllers();

            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}