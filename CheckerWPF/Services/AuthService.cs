using System.Net.Http;
using System.Net.Http.Json;
using CheckerWPF.Models;

namespace CheckerWPF.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        public string? Token { get; private set; }
        public string? Username { get; private set; }
        public string? Role { get; private set; }
        public bool IsLoggedIn => Token is not null;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/auth/login",
                    new LoginRequest(username, password));

                if (!resp.IsSuccessStatusCode) return false;

                var result = await resp.Content.ReadFromJsonAsync<LoginResponse>();
                if (result is null) return false;

                Token = result.Token;
                Username = result.Username;
                Role = result.Role;

                // Gắn token vào mọi request sau này
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            Token = null;
            Username = null;
            Role = null;
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }
}
