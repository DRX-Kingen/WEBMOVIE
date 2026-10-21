using System.Text.Json;
using MovieStreaming.DTOs;
using System.Text.Json;
using MovieStreaming.DTOs;
namespace MovieStreaming.Services;

public class VsmovService
{
    private readonly HttpClient _http;

    public VsmovService(HttpClient http)
    {
        _http = http;
    }
    
    public async Task<JsonElement> GetRawDetailAsync(string slug)
    {
        using var response = await _http.GetAsync(
            $"phim/{Uri.EscapeDataString(slug)}"
        );

        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync()
        );

        return doc.RootElement.Clone();
    }

    public async Task<VsmovMovieDetailDto> GetDetailAsync(string slug)
    {
        var root = await GetRawDetailAsync(slug);

        // Một số API bọc dữ liệu trong "data".
        var container = root;

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("data", out var data) &&
            data.ValueKind == JsonValueKind.Object)
        {
            container = data;
        }

        if (container.ValueKind != JsonValueKind.Object ||
            !container.TryGetProperty("movie", out var movie) ||
            movie.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException(
                "Không tìm thấy đối tượng movie trong JSON."
            );
        }

        var result = new VsmovMovieDetailDto
        {
            Name = ReadString(movie, "name") ?? "",
            Slug = ReadString(movie, "slug") ?? slug,
            OriginName = ReadString(movie, "origin_name"),
            Content = ReadString(movie, "content"),
            PosterUrl = ReadString(movie, "poster_url"),
            ThumbUrl = ReadString(movie, "thumb_url"),
            Year = ReadInt(movie, "year"),
            Time = ReadString(movie, "time"),
            EpisodeCurrent = ReadString(movie, "episode_current"),
            Categories = ReadNames(movie, "category"),
            Countries = ReadNames(movie, "country")
        };

        if (container.TryGetProperty("episodes", out var episodes) &&
            episodes.ValueKind == JsonValueKind.Array)
        {
            foreach (var server in episodes.EnumerateArray())
            {
                if (server.ValueKind != JsonValueKind.Object)
                    continue;

                var serverDto = new VsmovEpisodeServerDto
                {
                    ServerName = ReadString(server, "server_name") ?? "Server"
                };

                if (server.TryGetProperty("server_data", out var items) &&
                    items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var episode in items.EnumerateArray())
                    {
                        if (episode.ValueKind != JsonValueKind.Object)
                            continue;

                        serverDto.Items.Add(new VsmovEpisodeDto
                        {
                            Name = ReadString(episode, "name") ?? "",
                            Slug = ReadString(episode, "slug") ?? "",
                            LinkEmbed = ReadString(episode, "link_embed")
                        });
                    }
                }

                result.Episodes.Add(serverDto);
            }
        }

        return result;
    }

    private static string? ReadString(JsonElement obj, string key)
    {
        if (obj.ValueKind != JsonValueKind.Object ||
            !obj.TryGetProperty(key, out var value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            _ => null
        };
    }

    private static int? ReadInt(JsonElement obj, string key)
    {
        var text = ReadString(obj, key);
        return int.TryParse(text, out var value) ? value : null;
    }

    private static List<string> ReadNames(JsonElement obj, string key)
    {
        var result = new List<string>();

        if (!obj.TryGetProperty(key, out var items) ||
            items.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var name = item.GetString();
                if (!string.IsNullOrWhiteSpace(name))
                    result.Add(name);
            }
            else if (item.ValueKind == JsonValueKind.Object)
            {
                var name = ReadString(item, "name");
                if (!string.IsNullOrWhiteSpace(name))
                    result.Add(name);
            }
        }

        return result;
    }

    public async Task<JsonElement> GetRawLatestAsync(
        int page = 1)
    {
        page = Math.Clamp(page, 1, 1000);

        using var response = await _http.GetAsync(
            $"danh-sach/phim-moi-cap-nhat?page={page}"
        );

        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync()
        );

        return doc.RootElement.Clone();
    }

    public async Task<List<VsmovMovieDto>> GetLatestAsync(
        int page = 1)
    {
        var root = await GetRawLatestAsync(page);

        // Hỗ trợ mảng items ở cấp gốc hoặc trong data.
        JsonElement items;

        if (root.TryGetProperty("items", out var directItems))
        {
            items = directItems;
        }
        else if (
            root.TryGetProperty("data", out var data) &&
            data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("items", out var nestedItems))
        {
            items = nestedItems;
        }
        else
        {
            throw new JsonException(
                "Không tìm thấy mảng items trong phản hồi VSMOV."
            );
        }

        if (items.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(
                "Trường items không phải mảng."
            );
        }

        var movies = new List<VsmovMovieDto>();

        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            var name = GetString(item, "name");
            var slug = GetString(item, "slug");

            if (string.IsNullOrWhiteSpace(slug))
                continue;

            movies.Add(new VsmovMovieDto
            {
                Name = name ?? "",
                Slug = slug,
                PosterUrl = GetString(item, "poster_url"),
                ThumbUrl = GetString(item, "thumb_url"),
                Year = GetInt(item, "year")
            });
        }

        return movies;
    }

    private static string? GetString(
        JsonElement item, string key)
    {
        if (!item.TryGetProperty(key, out var value))
            return null;

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    public async Task<List<VsmovMovieDto>> SearchAsync(
        string keyword,
        int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<VsmovMovieDto>();
        }

        keyword = keyword.Trim();
        limit = Math.Clamp(limit, 1, 50);

        // Tạo URL tìm kiếm
        var url =
            $"tim-kiem?keyword={Uri.EscapeDataString(keyword)}&limit={limit}";

        // SỬA: sử dụng _http thay vì _httpClient
        using var response = await _http.GetAsync(url);

        response.EnsureSuccessStatusCode();

        using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var root = document.RootElement;

        JsonElement items = default;

        // Trường hợp 1: API trả về mảng trực tiếp
        if (root.ValueKind == JsonValueKind.Array)
        {
            items = root;
        }
        // Trường hợp 2: API trả về object
        else if (root.ValueKind == JsonValueKind.Object)
        {
            // JSON dạng { "items": [...] }
            if (
                root.TryGetProperty("items", out var directItems) &&
                directItems.ValueKind == JsonValueKind.Array
            )
            {
                items = directItems;
            }
            // JSON dạng { "data": [...] }
            // hoặc { "data": { "items": [...] } }
            else if (root.TryGetProperty("data", out var data))
            {
                if (data.ValueKind == JsonValueKind.Array)
                {
                    items = data;
                }
                else if (
                    data.ValueKind == JsonValueKind.Object &&
                    data.TryGetProperty("items", out var nestedItems) &&
                    nestedItems.ValueKind == JsonValueKind.Array
                )
                {
                    items = nestedItems;
                }
            }
        }

        if (items.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(
                "Không nhận diện được cấu trúc JSON tìm kiếm VSMOV."
            );
        }

        var movies = new List<VsmovMovieDto>();

        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            var slug = ReadString(item, "slug");

            // Bỏ qua phim không có slug
            if (string.IsNullOrWhiteSpace(slug))
                continue;

            movies.Add(new VsmovMovieDto
            {
                Name = ReadString(item, "name") ?? "",
                Slug = slug,
                PosterUrl = ReadString(item, "poster_url"),
                ThumbUrl = ReadString(item, "thumb_url"),
                Year = ReadInt(item, "year")
            });
        }

        return movies;
    }

    private static int? GetInt(
        JsonElement item, string key)
    {
        if (!item.TryGetProperty(key, out var value))
            return null;

        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt32(out var number))
            return number;

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(value.GetString(), out var parsed))
            return parsed;

        return null;
    }
}