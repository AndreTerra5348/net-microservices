using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpCommandDataClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendPlatformToCommand(PlatformReadDto platform)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(platform),
                Encoding.UTF8,
                "application/json");
            var response = await _httpClient.PostAsync(_configuration["CommandService"], content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("--> Sync POST to CommandService Ok!");
            }
            else
            {
                System.Console.WriteLine($"--> Sync POST to CommandService Error: {response.StatusCode}");
            }
        }
    }
}