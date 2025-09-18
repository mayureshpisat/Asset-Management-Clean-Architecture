using Application.Interfaces;
using Application.Services;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using System.Runtime.CompilerServices;

namespace Asset_Management.Extensions
{
    public static class AssetHierarchyServiceExtension 
    {
        public static IServiceCollection AddAssetHierarchyService(this IServiceCollection service, IConfiguration configuration) 
        {
            string serviceType = configuration["HierarchyServiceFlag"].ToLower();

            if (serviceType == "db")
            {
                service.AddScoped<IAssetHierarchyService, DbAssetHierarchyService>();
                service.AddScoped<IAssetRepository, AssetRepository>();
                service.AddScoped<ISignalRepository, SignalRepository>();
                //Notification service (singleton/ is stateless)
                service.AddSingleton<INotificationService, NotificationService>();
                //log service
                service.AddScoped<IAssetLogService, AssetLogService>();

                //user services
                service.AddScoped<IUserRepository, UserRepository>();
                service.AddScoped<IUserService, UserService>();
            }
            //else
            //{
            //    service.AddScoped<IAssetHierarchyService, AssetHierarchyService>();
            //}
            return service;

        }
    }
}
