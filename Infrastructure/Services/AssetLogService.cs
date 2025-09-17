using Infrastructure.Persistence;
using Application.Interfaces;
using Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


namespace Infrastructure.Services
{
    public class AssetLogService : IAssetLogService
    {
        private readonly AssetDbContext _dbContext;

        //By default httpContext is available only to controllers and middleware, IHttpContextAccessor is 
        //a special service that provides it outside of controllers and middleware.
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AssetLogService(AssetDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            
        }

        public async Task Log(string action, string? asset = null, string? signal = null)
        {
            if (string.IsNullOrWhiteSpace(asset))
                asset = string.Empty;

            if (string.IsNullOrWhiteSpace(signal))
                signal = string.Empty;

            Console.WriteLine($"Action : {action}");


            int.TryParse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int user);
            Console.WriteLine($"User : {user}");

            AssetLog assetLog = new AssetLog
            {
                UserId = user,
                Action = action,
                Asset = asset,
                Signal = signal,
                LogTime = DateTime.UtcNow

            };
            await _dbContext.AssetLogs.AddAsync(assetLog);
            await _dbContext.SaveChangesAsync();

            Console.WriteLine($"{action} LOGGED");


        }

    }

}
