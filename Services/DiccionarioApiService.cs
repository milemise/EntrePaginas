
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace EntrePaginas.Services;

public class DefinicionDiccionario
{
    public string Palabra { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public List<string> Definiciones { get; set; } = new();
    public List<string> Ejemplos { get; set; } = new();
    public string UrlFuente { get; set; } = string.Empty;
}

public class DiccionarioApiService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;

    private const string BaseUrl = "https://es.wiktionary.org/w/api.php";

    public DiccionarioApiService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
        _http.Timeout = TimeSpan.FromSeconds(15);

        if (!_http.DefaultRequestHeaders.UserAgent.Any())
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "EntrePaginas/1.0 (proyecto educativo)");
        }
    }

    public async Task<DefinicionDiccionario?> BuscarAsync(string palabra)
    {
        palabra = (palabra ?? string.Empty).Trim();

        if (palabra.Length == 0 || palabra.Length > 150)
            return null;

        var clave = "diccionario-es-v3-" + palabra.ToLowerInvariant();

        if (_cache.TryGetValue(clave, out DefinicionDiccionario? guardada))
            return guardada;

        var url = BaseUrl
            + "?action=query&format=json&formatversion=2"
            + "&prop=extracts&explaintext=1&redirects=1"
            + "&titles=" + Uri.EscapeDataString(palabra);

        using var respuesta = await _http.GetAsync(url);
        respuesta.EnsureSuccessStatusCode();

        await using var stream = await respuesta.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        if (!json.RootElement.TryGetProperty("query", out var query) ||
            !query.TryGetProperty("pages", out var pages) ||
            pages.ValueKind != JsonValueKind.Array ||
            pages.GetArrayLength() == 0)
        {
            return null;
        }

        var pagina = pages[0];

        if (pagina.TryGetProperty("missing", out _))
            return null;

        var titulo = pagina.TryGetProperty("title", out var tituloJson)
            ? tituloJson.GetString() ?? palabra
            : palabra;

        var contenido = pagina.TryGetProperty("extract", out var extractJson)
            ? extractJson.GetString() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrWhiteSpace(contenido))
            return null;

        var definiciones = ExtraerDefiniciones(contenido);
        var ejemplos = ExtraerEjemplos(contenido);

        var resultado = new DefinicionDiccionario
        {
            Palabra = titulo,
            Contenido = definiciones.FirstOrDefault()
                ?? "No se encontró una definición breve para esta palabra.",
            Definiciones = definiciones,
            Ejemplos = ejemplos,
            UrlFuente = "https://es.wiktionary.org/wiki/"
                + Uri.EscapeDataString(titulo.Replace(' ', '_'))
        };

        _cache.Set(clave, resultado, TimeSpan.FromHours(12));

        return resultado;
    }

    
    private static List<string> ExtraerDefiniciones(string contenido)
    {
        var seccion = Regex.Match(
            contenido,
            @"===\s*(?:Sustantivo|Verbo|Adjetivo|Adverbio|Interjección|Pronombre|Preposición)[^=]*===\s*(?<texto>.*?)(?====|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (!seccion.Success)
            return new List<string>();

        var texto = seccion.Groups["texto"].Value;
        var definiciones = new List<string>();

        texto = Regex.Replace(texto, @"(?m)^\s*#:\s*.*$", "");

        var coincidencias = Regex.Matches(
            texto,
            @"(?m)^\s*#\s*(?![:*])(.+)$");

        foreach (Match coincidencia in coincidencias)
        {
            var definicion = LimpiarDefinicion(
                coincidencia.Groups[1].Value);

            if (definicion.Length < 15)
                continue;

            if (Regex.IsMatch(
                definicion,
                @"^(?:[^.¦]{1,60}\s*¦\s*)?(?:plural|femenino|masculino)\s*:",
                RegexOptions.IgnoreCase))
            {
                continue;
            }

            if (!definiciones.Contains(
                definicion,
                StringComparer.OrdinalIgnoreCase))
            {
                definiciones.Add(definicion);
            }
        }

        if (definiciones.Count == 0)
        {
            var partes = Regex.Split(
                texto,
                @"(?<!\S)\d+\s+(?=[A-ZÁÉÍÓÚÜÑ])");

            foreach (var parte in partes)
            {
                var definicion = LimpiarDefinicion(parte);

                if (definicion.Length >= 15 &&
                    !Regex.IsMatch(
                        definicion,
                        @"\b(?:plural|locuciones|traducciones|etimología|referencias)\b",
                        RegexOptions.IgnoreCase))
                {
                    definiciones.Add(definicion);
                }
            }
        }

        return definiciones.Take(8).ToList();
    }

    private static string LimpiarDefinicion(string texto)
    {
        texto = Regex.Replace(texto, @"\[\d+\]", "");
        texto = Regex.Replace(texto, @"\s+", " ").Trim();

        texto = Regex.Split(
            texto,
            @"\b(?:Ámbito|Sinónimos?|Antónimos?|Categoría gramatical)\s*:",
            RegexOptions.IgnoreCase)[0];

        return texto.Trim(' ', '.', ';', ':', '-', '¦');
    }


    private static List<string> ExtraerEjemplos(string contenido)
    {
        var ejemplos = new List<string>();

        var coincidencias = Regex.Matches(
            contenido,
            @"(?:Ejemplo|Ejemplos)\s*:\s*([^.\n]+[.!?])",
            RegexOptions.IgnoreCase);

        foreach (Match coincidencia in coincidencias)
        {
            var ejemplo = coincidencia.Groups[1].Value.Trim();

            if (ejemplo.Length > 5 &&
                !ejemplos.Contains(
                    ejemplo,
                    StringComparer.OrdinalIgnoreCase))
            {
                ejemplos.Add(ejemplo);
            }
        }

        return ejemplos.Take(3).ToList();
    }
}
