using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CapstonePlaywrightTesting19.Configuration;

namespace CapstonePlaywrightTesting19.Utilities;

/// <summary>
/// Thin client for the backend API used by demo1. Used only for test setup / data lookup
/// (provisioning an account, finding a showtime that still has free seats) – never for assertions
/// that the UI is supposed to cover.
/// </summary>
public sealed class MovieApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _group;

    public MovieApiClient()
    {
        var settings = ConfigManager.Settings;
        _group = settings.GroupCode;
        _http = new HttpClient
        {
            BaseAddress = new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromMilliseconds(settings.Timeouts.NavigationMs)
        };
    }

    public async Task RegisterAsync(UserAccount account)
    {
        var payload = new
        {
            taiKhoan = account.Username,
            matKhau = account.Password,
            email = account.Email,
            hoTen = account.FullName,
            soDt = "0900000000",
            maNhom = _group,
            maLoaiNguoiDung = "KhachHang"
        };

        var response = await _http.PostAsJsonAsync("QuanLyNguoiDung/DangKy", payload);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Account provisioning failed ({(int)response.StatusCode}): {body}");
        }

        Logger.Info($"Provisioned test account '{account.Username}' through the API");
    }

    public async Task<List<MovieInfo>> GetMoviesAsync()
    {
        var json = await GetJsonAsync($"QuanLyPhim/LayDanhSachPhim?maNhom={_group}");
        return json!.AsArray()
            .Select(n => new MovieInfo((int)n!["maPhim"]!, (string)n["tenPhim"]!))
            .ToList();
    }

    /// <summary>All showtimes of a movie, flattened from the cinema-system → cinema → showtime tree.</summary>
    public async Task<List<ShowtimeInfo>> GetShowtimesAsync(MovieInfo movie)
    {
        var json = await GetJsonAsync($"QuanLyRap/LayThongTinLichChieuPhim?MaPhim={movie.Id}");
        var result = new List<ShowtimeInfo>();
        foreach (var system in json?["heThongRapChieu"]?.AsArray() ?? new JsonArray())
        foreach (var cinema in system?["cumRapChieu"]?.AsArray() ?? new JsonArray())
        foreach (var showtime in cinema?["lichChieuPhim"]?.AsArray() ?? new JsonArray())
        {
            result.Add(new ShowtimeInfo(
                movie,
                (string)cinema!["maCumRap"]!,
                (string)cinema["tenCumRap"]!,
                int.Parse(showtime!["maLichChieu"]!.ToString())));
        }

        return result;
    }

    /// <summary>First showtime of the first movie in the catalogue that has any showtime at all.</summary>
    public async Task<ShowtimeInfo> FindAnyShowtimeAsync()
    {
        foreach (var movie in await GetMoviesAsync())
        {
            var showtimes = await GetShowtimesAsync(movie);
            if (showtimes.Count > 0) return showtimes[0];
        }

        throw new InconclusiveException("No movie with showtimes is available on the demo server.");
    }

    public async Task<int> CountFreeSeatsAsync(int showtimeId)
    {
        var json = await GetJsonAsync($"QuanLyDatVe/LayDanhSachPhongVe?MaLichChieu={showtimeId}");
        return json?["danhSachGhe"]?.AsArray().Count(seat => !(bool)seat!["daDat"]!) ?? 0;
    }

    /// <summary>Walks the catalogue and returns the first showtime that still has free seats.</summary>
    public async Task<ShowtimeInfo> FindShowtimeWithFreeSeatsAsync(int minFreeSeats = 2, IEnumerable<int>? preferredIds = null)
    {
        foreach (var preferred in preferredIds ?? Enumerable.Empty<int>())
        {
            if (await CountFreeSeatsAsync(preferred) >= minFreeSeats)
            {
                return new ShowtimeInfo(new MovieInfo(0, string.Empty), string.Empty, string.Empty, preferred);
            }
        }

        foreach (var movie in await GetMoviesAsync())
        foreach (var showtime in await GetShowtimesAsync(movie))
        {
            if (await CountFreeSeatsAsync(showtime.ShowtimeId) >= minFreeSeats)
            {
                return showtime;
            }
        }

        throw new InconclusiveException("No showtime with free seats is available on the demo server.");
    }

    private async Task<JsonNode?> GetJsonAsync(string relativeUrl)
    {
        Logger.Debug($"API GET {relativeUrl}");
        var response = await _http.GetAsync(relativeUrl);
        response.EnsureSuccessStatusCode();
        return JsonNode.Parse(await response.Content.ReadAsStringAsync());
    }

    public void Dispose() => _http.Dispose();
}

public sealed record MovieInfo(int Id, string Name);

public sealed record ShowtimeInfo(MovieInfo Movie, string CinemaId, string CinemaName, int ShowtimeId);

public sealed record UserAccount(string Username, string Password, string Email, string FullName)
{
    public static UserAccount Random() =>
        new(DataGenerator.Username(), DataGenerator.Password(), DataGenerator.Email(), DataGenerator.FullName());
}
