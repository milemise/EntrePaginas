using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace EntrePaginas.Services;

public sealed class OpenLibraryService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public OpenLibraryService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<IReadOnlyList<OpenLibraryBook>> BuscarAsync(string termino, int limite = 10, CancellationToken cancellationToken = default)
    {
        termino = (termino ?? string.Empty).Trim();
        if (termino.Length < 2) return Array.Empty<OpenLibraryBook>();
        limite = Math.Clamp(limite, 1, 12);
        var cacheKey = $"openlibrary:{termino.ToLowerInvariant()}:{limite}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<OpenLibraryBook>? guardados) && guardados is not null) return guardados;
        var uri = $"search.json?q={Uri.EscapeDataString(termino)}&fields=key,title,author_name,first_publish_year,cover_i,subject,first_sentence&limit={limite}&lang=es";
        using var response = await _httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var resultado = await JsonSerializer.DeserializeAsync<SearchResponse>(stream, JsonOptions, cancellationToken);
        IReadOnlyList<OpenLibraryBook> libros = (resultado?.Docs ?? new List<SearchDocument>())
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .Select(x => new OpenLibraryBook
            {
                Key = x.Key ?? string.Empty,
                Titulo = x.Title!.Trim(),
                Autor = x.AuthorName is { Count: > 0 } ? string.Join(", ", x.AuthorName.Take(3)) : "Autor no informado",
                AnioPublicacion = x.FirstPublishYear,
                PortadaUrl = x.CoverId.HasValue ? $"https://covers.openlibrary.org/b/id/{x.CoverId.Value}-M.jpg" : null,
                Descripcion = x.FirstSentence is { Count: > 0 } ? string.Join(" ", x.FirstSentence.Take(2)) : null,
                Temas = x.Subject?.Take(4).ToArray() ?? Array.Empty<string>()
            }).ToList();
        _cache.Set(cacheKey, libros, TimeSpan.FromMinutes(10));
        return libros;
    }

    private sealed class SearchResponse
    {
        [JsonPropertyName("docs")] public List<SearchDocument>? Docs { get; set; }
    }

    private sealed class SearchDocument
    {
        [JsonPropertyName("key")] public string? Key { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("author_name")] public List<string>? AuthorName { get; set; }
        [JsonPropertyName("first_publish_year")] public int? FirstPublishYear { get; set; }
        [JsonPropertyName("cover_i")] public int? CoverId { get; set; }
        [JsonPropertyName("subject")] public List<string>? Subject { get; set; }
        [JsonPropertyName("first_sentence")] public List<string>? FirstSentence { get; set; }
    }
}

public sealed class OpenLibraryBook
{
    public string Key { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public int? AnioPublicacion { get; set; }
    public string? PortadaUrl { get; set; }
    public string? Descripcion { get; set; }
    public string[] Temas { get; set; } = Array.Empty<string>();
}
