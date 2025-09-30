using Application.Interfaces;
using BackgroundCalculatorService;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "BackgroundCalculatorService";
});

builder.Services.AddSignalR();

builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient();


builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddDbContext<AssetDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});










var host = builder.Build();
host.Run();
