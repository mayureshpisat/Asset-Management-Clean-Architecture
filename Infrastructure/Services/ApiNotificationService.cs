using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ApiNotificationService
    {
        private readonly HttpClient _httpClient;
        public ApiNotificationService(HttpClient httpClient) 
        {
            _httpClient = httpClient;

        }

        public async Task SendStatsToEveryone(double temp, double power)
        {
            var payload = new { Temperature = temp, Power = power };
            //await _httpClient.PostAsJsonAsync("https://localhost:7242/api/AssetHierarchy/SendFromWorker", payload);
        }
    }
}
