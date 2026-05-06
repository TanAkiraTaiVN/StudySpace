using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StudySpace.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly AuthState _auth;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public string BaseUrl { get; private set; }

    public ApiService(AuthState auth)
    {
        _auth = auth;
        BaseUrl = Preferences.Get("api_base", DefaultBase());
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    private static string DefaultBase()
    {
#if ANDROID
        return "http://10.0.2.2:5000";
#else
        return "http://localhost:5000";
#endif
    }

    public void SetBaseUrl(string url)
    {
        BaseUrl = url.TrimEnd('/');
        Preferences.Set("api_base", BaseUrl);
    }

    private HttpRequestMessage Build(HttpMethod method, string path, HttpContent? content = null)
    {
        var req = new HttpRequestMessage(method, BaseUrl.TrimEnd('/') + path);
        if (content != null) req.Content = content;
        if (!string.IsNullOrEmpty(_auth.Token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);
        return req;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string path)
    {
        using var resp = await _http.SendAsync(Build(HttpMethod.Get, path));
        return await ParseAsync<T>(resp);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string path, object? body = null)
    {
        var content = body == null ? null : JsonContent.Create(body);
        using var resp = await _http.SendAsync(Build(HttpMethod.Post, path, content));
        return await ParseAsync<T>(resp);
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string path, object? body = null)
    {
        var content = body == null ? null : JsonContent.Create(body);
        using var resp = await _http.SendAsync(Build(HttpMethod.Put, path, content));
        return await ParseAsync<T>(resp);
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string path)
    {
        using var resp = await _http.SendAsync(Build(HttpMethod.Delete, path));
        return await ParseAsync<T>(resp);
    }

    private static async Task<ApiResponse<T>> ParseAsync<T>(HttpResponseMessage resp)
    {
        var text = await resp.Content.ReadAsStringAsync();
        try
        {
            var obj = JsonSerializer.Deserialize<ApiResponse<T>>(text, JsonOpts);
            if (obj != null) return obj;
        }
        catch { /* fall through */ }
        return new ApiResponse<T> { Success = false, Message = $"HTTP {(int)resp.StatusCode}: {text}" };
    }
}
