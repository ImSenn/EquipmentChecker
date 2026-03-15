using System.Net.Http;
using System.Net.Http.Json;
using CheckerWPF.Models;

namespace CheckerWPF.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        // ── Devices ──────────────────────────────────────────────────────
        public Task<List<DeviceDto>?> GetDevicesAsync()
            => _http.GetFromJsonAsync<List<DeviceDto>>("api/devices");

        public Task<DeviceDto?> GetDeviceAsync(string id)
            => _http.GetFromJsonAsync<DeviceDto>($"api/devices/{id}");

        // ── Telemetry history ─────────────────────────────────────────────
        public Task<List<TelemetryDto>?> GetTelemetryAsync(
            string deviceId, DateTime from, DateTime to, int limit = 200)
        {
            var url = $"api/devices/{deviceId}/telemetry" +
                      $"?from={from:o}&to={to:o}&limit={limit}";
            return _http.GetFromJsonAsync<List<TelemetryDto>>(url);
        }

        // ── Events ────────────────────────────────────────────────────────
        public Task<List<EventDto>?> GetEventsAsync(string deviceId)
            => _http.GetFromJsonAsync<List<EventDto>>($"api/devices/{deviceId}/events");

        // ── Energy ────────────────────────────────────────────────────────
        public async Task<EnergyDto?> GetEnergyAsync(string deviceId, DateTime date)
        {
            try
            {
                return await _http.GetFromJsonAsync<EnergyDto>(
                    $"api/devices/{deviceId}/energy?date={date:yyyy-MM-dd}");
            }
            catch (HttpRequestException)
            {
                return null; // 404 = không có dữ liệu hôm đó
            }
        }

        // ── Command ───────────────────────────────────────────────────────
        public async Task<bool> SendCommandAsync(string deviceId, CommandRequest cmd)
        {
            var resp = await _http.PostAsJsonAsync(
                $"api/devices/{deviceId}/command", cmd);
            return resp.IsSuccessStatusCode;
        }
    }
}
