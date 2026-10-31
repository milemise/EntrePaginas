
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace EntrePaginas.Services;

public class DefinicionDiccionario
{
    public string Palabra { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public List<string> Definiciones { get; set; } = new();
    public List<string> Ejemplos { get; set; } = new();
    public string UrlFuente { get; set; } = string.Empty;
    public string Origen { get; set; } = "Wikcionario en español";
}

public sealed class DiccionarioTemporalmenteNoDisponibleException : Exception
{
    public DiccionarioTemporalmenteNoDisponibleException() : base("No hay fuentes de diccionario disponibles en este momento.") { }
}

public class DiccionarioApiService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;

    private const string BaseUrl = "https://es.wiktionary.org/w/api.php";

    private static readonly IReadOnlyDictionary<string, DefinicionDiccionario> GlosarioLiterario =
        new Dictionary<string, DefinicionDiccionario>(StringComparer.OrdinalIgnoreCase)
        {
            ["metafora"] = CrearEntrada("Metáfora", "Figura retórica que identifica una cosa con otra por una semejanza entre ambas, sin utilizar un nexo comparativo.", "En la novela, la protagonista llamó al recuerdo «una casa sin puertas».", "La metáfora permite expresar una idea mediante una imagen con la que guarda una relación de sentido."),
            ["hiperbole"] = CrearEntrada("Hipérbole", "Figura retórica que exagera deliberadamente una característica, una emoción o una situación para intensificar su efecto.", "Te esperé mil años bajo la lluvia.", "La hipérbole no busca describir una cantidad literal, sino destacar una sensación o una idea."),
            ["analepsis"] = CrearEntrada("Analepsis", "Recurso narrativo que interrumpe el presente del relato para contar un hecho ocurrido en un momento anterior.", "Al ver la fotografía, recordó la tarde en que había conocido a su mejor amiga.", "También se conoce como retrospección o flashback."),
            ["protagonista"] = CrearEntrada("Protagonista", "Personaje principal de una historia, alrededor de cuyos objetivos, decisiones o conflictos se organiza buena parte de la narración.", "La protagonista emprende un viaje para descubrir quién escribió el manuscrito.", "El protagonista no tiene que ser necesariamente una persona ni ser presentado como alguien heroico."),
            ["narrador omnisciente"] = CrearEntrada("Narrador omnisciente", "Voz narrativa que conoce los hechos y puede acceder a los pensamientos, sentimientos o recuerdos de varios personajes.", "Clara sonrió, sin saber que Julián ocultaba una noticia que cambiaría sus vidas.", "El narrador omnisciente suele relatar en tercera persona y ofrece información que los personajes desconocen."),
            ["personificacion"] = CrearEntrada("Personificación", "Figura retórica que atribuye características, acciones o sentimientos humanos a animales, objetos o ideas abstractas.", "La ciudad despertó de mal humor y bostezó entre sus avenidas.", "También recibe el nombre de prosopopeya."),
            ["simil"] = CrearEntrada("Símil", "Figura retórica que compara dos elementos mediante expresiones como «como», «parece» o «igual que».", "Sus palabras eran suaves como la brisa de verano.", "A diferencia de la metáfora, el símil expresa de manera directa la comparación."),
            ["climax"] = CrearEntrada("Clímax", "Momento de mayor tensión, intensidad o importancia dentro de una narración o de una secuencia dramática.", "El clímax llegó cuando la carta reveló quién había escrito el diario.", "Después del clímax, la historia suele avanzar hacia la resolución del conflicto."),
            ["antagonista"] = CrearEntrada("Antagonista", "Personaje, fuerza o circunstancia que se opone al objetivo del protagonista y genera parte del conflicto de una historia.", "La investigadora tuvo que enfrentarse a una institución que intentaba ocultar la verdad.", "El antagonista no necesariamente es malvado; su función es oponerse al objetivo central."),
            ["prologo"] = CrearEntrada("Prólogo", "Texto que precede al comienzo principal de una obra y presenta información, contexto o una perspectiva que ayuda a iniciar la lectura.", "El prólogo cuenta cómo se encontró el cuaderno que da origen a la novela.", "En algunas obras el prólogo está escrito por una persona distinta de quien escribió el libro."),
            ["epilogo"] = CrearEntrada("Epílogo", "Sección final de una obra que presenta consecuencias, cierra aspectos de la historia o muestra qué ocurrió después del desenlace.", "El epílogo descubre dónde vive cada personaje diez años más tarde.", "No todas las obras necesitan un epílogo."),
            ["ironia"] = CrearEntrada("Ironía", "Recurso expresivo en el que el sentido pretendido difiere del significado literal, a menudo para señalar un contraste o producir humor o crítica.", "Justo cuando empezó a llover, dijo: «Qué día perfecto para un picnic».", "La interpretación depende del contexto y de la intención comunicativa."),
            ["alegoria"] = CrearEntrada("Alegoría", "Representación continuada de una idea abstracta mediante personajes, situaciones o imágenes concretas que adquieren un sentido simbólico.", "Un relato sobre un jardín dividido puede representar una sociedad enfrentada.", "Las alegorías suelen desarrollar el sentido simbólico a lo largo de una obra, no en una sola frase."),
            ["fabula"] = CrearEntrada("Fábula", "Relato breve, en prosa o verso, que presenta una situación ficticia y suele concluir con una enseñanza o moraleja.", "Una fábula puede mostrar a una tortuga y una liebre para reflexionar sobre la constancia.", "En las fábulas, los animales y objetos pueden actuar como personajes."),
            ["desenlace"] = CrearEntrada("Desenlace", "Parte final de una narración en la que se resuelve o se cierra el conflicto principal, aunque el cierre también puede ser abierto.", "En el desenlace, los hermanos finalmente descubren el origen de la carta.", "El desenlace no tiene que resolver todas las preguntas que plantea una obra."),
            ["narrativa"] = CrearEntrada("Narrativa", "Forma de expresión que cuenta una serie de acontecimientos mediante una voz narrativa, con personajes, tiempo y espacio.", "La novela y el cuento son formas habituales de narrativa.", "La narrativa puede aparecer en libros, cine, historietas, videojuegos y otros medios."),
            ["nostalgia"] = CrearEntrada("Nostalgia", "Sentimiento de afecto, añoranza o tristeza al recordar una persona, un lugar o una etapa del pasado.", "El olor del pan recién hecho le despertó nostalgia por la casa de su infancia.", "En literatura, la nostalgia puede funcionar como tema, tono o motivación de un personaje."),
            ["serendipia"] = CrearEntrada("Serendipia", "Hallazgo valioso o afortunado que ocurre de manera inesperada mientras se buscaba otra cosa o se realizaba otra actividad.", "Buscaba una dirección y, por serendipia, encontró la librería que llevaba años imaginando.", "Se utiliza para destacar un descubrimiento casual con un resultado positivo."),
            ["efimero"] = CrearEntrada("Efímero", "Que dura poco tiempo o tiene una existencia breve.", "La luz efímera del atardecer iluminó la página por unos segundos.", "Se emplea con frecuencia para hablar de experiencias, momentos o cosas pasajeras.")
        };

    private static DefinicionDiccionario CrearEntrada(string palabra, string definicion, string ejemplo, string contenido) => new()
    {
        Palabra = palabra,
        Contenido = contenido,
        Definiciones = new List<string> { definicion },
        Ejemplos = new List<string> { ejemplo },
        UrlFuente = string.Empty,
        Origen = "Glosario literario de Entre Páginas"
    };

    private static string NormalizarClave(string valor)
    {
        var descompuesto = valor.Normalize(NormalizationForm.FormD);
        var salida = new StringBuilder(descompuesto.Length);
        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                salida.Append(caracter);
        }
        return Regex.Replace(salida.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant(), @"\s+", " ").Trim();
    }

    public DiccionarioApiService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
        _http.Timeout = TimeSpan.FromSeconds(8);

        // Wikimedia puede rechazar clientes con User-Agent genérico o sin identificación.
        _http.DefaultRequestHeaders.UserAgent.Clear();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "EntrePaginas/1.1 (+https://github.com/milemise/EntrePaginas)");
        if (!_http.DefaultRequestHeaders.Accept.Any())
            _http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }

    public async Task<DefinicionDiccionario?> BuscarAsync(
        string palabra,
        CancellationToken cancellationToken = default)
    {
        palabra = (palabra ?? string.Empty).Trim();
        palabra = Regex.Replace(palabra, @"\s+", " ");

        if (palabra.Length == 0 || palabra.Length > 150)
            return null;

        var clave = "diccionario-es-v5-" + NormalizarClave(palabra);
        if (_cache.TryGetValue(clave, out DefinicionDiccionario? guardada))
            return guardada;

        if (GlosarioLiterario.TryGetValue(NormalizarClave(palabra), out var entradaLocal))
        {
            _cache.Set(clave, entradaLocal, TimeSpan.FromHours(12));
            return entradaLocal;
        }

        var falloDeServicio = false;
        DefinicionDiccionario? resultado = null;

        // Primera fuente: Wikcionario en español.
        try
        {
            resultado = await BuscarEnWikcionarioAsync(palabra, cancellationToken);
        }
        catch (HttpRequestException)
        {
            falloDeServicio = true;
        }
        catch (JsonException)
        {
            falloDeServicio = true;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            falloDeServicio = true;
        }

        if (resultado is not null)
        {
            _cache.Set(clave, resultado, TimeSpan.FromHours(12));
            return resultado;
        }

        if (cancellationToken.IsCancellationRequested)
            return null;

        // Segunda fuente: definiciones en español del DLE mediante RAE API.
        // Es un respaldo para que una caída o un rechazo de Wikcionario no deje vacía la búsqueda.
        try
        {
            resultado = await BuscarEnRaeAsync(palabra, cancellationToken);
        }
        catch (HttpRequestException)
        {
            falloDeServicio = true;
        }
        catch (JsonException)
        {
            falloDeServicio = true;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            falloDeServicio = true;
        }

        if (resultado is not null)
        {
            _cache.Set(clave, resultado, TimeSpan.FromHours(12));
            return resultado;
        }

        if (falloDeServicio)
            throw new DiccionarioTemporalmenteNoDisponibleException();

        return null;
    }

    private async Task<DefinicionDiccionario?> BuscarEnWikcionarioAsync(
        string palabra,
        CancellationToken cancellationToken)
    {
        var url = BaseUrl
            + "?action=query&format=json&formatversion=2"
            + "&prop=extracts&explaintext=1&exsectionformat=plain&redirects=1&uselang=es"
            + "&titles=" + Uri.EscapeDataString(palabra);

        using var respuesta = await _http.GetAsync(url, cancellationToken);
        if (!respuesta.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Wikcionario devolvió HTTP {(int)respuesta.StatusCode}.",
                null,
                respuesta.StatusCode);

        await using var stream = await respuesta.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!json.RootElement.TryGetProperty("query", out var query) ||
            !query.TryGetProperty("pages", out var pages) ||
            pages.ValueKind != JsonValueKind.Array ||
            pages.GetArrayLength() == 0)
            return null;

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

        // No aceptar cualquier párrafo del artículo como si fuera una definición:
        // puede ser una etimología, pronunciación o información de otro idioma.
        // Si no se pudo extraer una acepción en español, se permite probar la fuente alternativa.
        var definiciones = ExtraerDefinicionesTextoPlano(contenido);
        if (definiciones.Count == 0)
            return null;

        return new DefinicionDiccionario
        {
            Palabra = titulo,
            Contenido = contenido.Length > 2500 ? contenido[..2500] + "…" : contenido,
            Definiciones = definiciones.Take(8).ToList(),
            Ejemplos = ExtraerEjemplos(contenido),
            UrlFuente = "https://es.wiktionary.org/wiki/" + Uri.EscapeDataString(titulo.Replace(' ', '_')),
            Origen = "Wikcionario en español"
        };
    }

    private async Task<DefinicionDiccionario?> BuscarEnRaeAsync(
        string palabra,
        CancellationToken cancellationToken)
    {
        var url = "https://rae-api.com/api/words/" + Uri.EscapeDataString(palabra);
        using var respuesta = await _http.GetAsync(url, cancellationToken);

        // 404 significa que esa palabra no está en esta fuente; otros errores se consideran fallos del servicio.
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        if (!respuesta.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"El servicio de respaldo devolvió HTTP {(int)respuesta.StatusCode}.",
                null,
                respuesta.StatusCode);

        await using var stream = await respuesta.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;

        if (!root.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True ||
            !root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            return null;

        var titulo = data.TryGetProperty("word", out var palabraJson)
            ? palabraJson.GetString() ?? palabra
            : palabra;
        var definiciones = new List<string>();
        var ejemplos = new List<string>();

        if (data.TryGetProperty("meanings", out var significados) &&
            significados.ValueKind == JsonValueKind.Array)
        {
            foreach (var significado in significados.EnumerateArray())
            {
                if (!significado.TryGetProperty("senses", out var acepciones) ||
                    acepciones.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var acepcion in acepciones.EnumerateArray())
                {
                    var texto = ObtenerTexto(acepcion, "raw", "description");
                    texto = LimpiarDefinicion(texto ?? string.Empty);
                    if (texto.Length >= 8 && !definiciones.Contains(texto, StringComparer.OrdinalIgnoreCase))
                        definiciones.Add(texto);

                    if (acepcion.TryGetProperty("examples", out var ejemplosJson) &&
                        ejemplosJson.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var ejemploJson in ejemplosJson.EnumerateArray())
                        {
                            var ejemplo = ejemploJson.ValueKind == JsonValueKind.String
                                ? ejemploJson.GetString()?.Trim()
                                : null;
                            if (!string.IsNullOrWhiteSpace(ejemplo) &&
                                !ejemplos.Contains(ejemplo, StringComparer.OrdinalIgnoreCase))
                                ejemplos.Add(ejemplo);
                        }
                    }
                }
            }
        }

        if (definiciones.Count == 0)
            return null;

        return new DefinicionDiccionario
        {
            Palabra = titulo,
            Contenido = string.Join("\n\n", definiciones.Take(8)),
            Definiciones = definiciones.Take(8).ToList(),
            Ejemplos = ejemplos.Take(3).ToList(),
            UrlFuente = "https://dle.rae.es/" + Uri.EscapeDataString(titulo.ToLowerInvariant()),
            Origen = "Diccionario de la lengua española"
        };
    }

    private static string? ObtenerTexto(JsonElement elemento, params string[] propiedades)
    {
        foreach (var propiedad in propiedades)
        {
            if (elemento.TryGetProperty(propiedad, out var valor) &&
                valor.ValueKind == JsonValueKind.String)
            {
                var texto = valor.GetString();
                if (!string.IsNullOrWhiteSpace(texto))
                    return texto;
            }
        }
        return null;
    }

    private static List<string> ExtraerDefinicionesTextoPlano(string contenido)
    {
        var definiciones = new List<string>();
        var lineas = contenido.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var haySeccionEspanol = lineas.Any(x => NormalizarClave(x) is "espanol" or "espanol (espana)");
        var dentroEspanol = !haySeccionEspanol;
        var dentroCategoria = false;

        foreach (var original in lineas)
        {
            var linea = Regex.Replace(original, @"\s+", " ").Trim();
            if (linea.Length == 0)
                continue;

            var normalizada = NormalizarClave(linea);

            if (EsEncabezadoIdioma(linea))
            {
                dentroEspanol = normalizada == "espanol" || normalizada.StartsWith("espanol ", StringComparison.Ordinal);
                dentroCategoria = false;
                continue;
            }

            if (!dentroEspanol)
                continue;

            // Wikcionario usa encabezados como "Sustantivo femenino",
            // "Sustantivo propio" o "Verbo transitivo", no solo "Sustantivo".
            if (EsCategoriaGramatical(normalizada))
            {
                dentroCategoria = true;
                continue;
            }

            if (EsEncabezadoDiccionario(normalizada))
            {
                dentroCategoria = false;
                continue;
            }

            var empiezaNumeracion = Regex.IsMatch(linea, @"^\s*\d+(?:[.)])?\s+");
            var definicion = Regex.Replace(linea, @"^\s*(?:[#*•]+\s*)?\d*(?:[.)])?\s*", "");
            definicion = Regex.Replace(definicion, @"^[#*•]+\s*", "").Trim();

            // En el extracto de MediaWiki el número y el ámbito ("1 Astronomía")
            // pueden aparecer en una línea y la definición en la siguiente.
            definicion = Regex.Replace(
                definicion,
                @"^(?:Astronomía|Peces|Botánica|Medicina|Zoología|Geografía|Química|Física|Mitología|Informática|Derecho|Lingüística|Literatura|Música|Matemáticas|Religión|Aragón|Argentina|México|España)\s+",
                "",
                RegexOptions.IgnoreCase);

            if (EsLineaDeMetadatos(definicion))
                continue;

            // Las definiciones pueden venir en una línea propia después del número.
            // También se contemplan entradas donde el texto está en la misma línea.
            if (!dentroCategoria && !empiezaNumeracion)
                continue;

            definicion = LimpiarDefinicion(definicion);
            if (definicion.Length < 8 || EsEncabezadoDiccionario(definicion))
                continue;

            if (!definiciones.Contains(definicion, StringComparer.OrdinalIgnoreCase))
                definiciones.Add(definicion);
        }

        return definiciones.Take(12).ToList();
    }

    private static bool EsLineaDeMetadatos(string texto)
    {
        var normalizada = NormalizarClave(texto);
        return normalizada.StartsWith("ejemplo:", StringComparison.Ordinal)
            || normalizada.StartsWith("ejemplos:", StringComparison.Ordinal)
            || normalizada.StartsWith("sinonimo:", StringComparison.Ordinal)
            || normalizada.StartsWith("sinonimos:", StringComparison.Ordinal)
            || normalizada.StartsWith("antonimo:", StringComparison.Ordinal)
            || normalizada.StartsWith("antonimos:", StringComparison.Ordinal)
            || normalizada.StartsWith("hiperonimo:", StringComparison.Ordinal)
            || normalizada.StartsWith("hiperonimos:", StringComparison.Ordinal)
            || normalizada.StartsWith("uso:", StringComparison.Ordinal)
            || normalizada.StartsWith("ambito:", StringComparison.Ordinal)
            || normalizada.StartsWith("plural:", StringComparison.Ordinal)
            || normalizada.StartsWith("femenino:", StringComparison.Ordinal)
            || normalizada.StartsWith("masculino:", StringComparison.Ordinal)
            || normalizada.StartsWith("derivados:", StringComparison.Ordinal)
            || normalizada.StartsWith("categoria gramatical:", StringComparison.Ordinal);
    }

    private static bool EsEncabezadoIdioma(string texto)
    {
        var normalizada = NormalizarClave(texto);
        return new[]
        {
            "espanol", "ingles", "frances", "italiano", "portugues", "latin", "griego",
            "aleman", "catalan", "gallego", "euskera", "arabe", "aragones", "chabacano",
            "occitano", "veneto", "mapudungun", "quechua", "nahuatl", "charrua"
        }.Any(idioma => normalizada == idioma || normalizada.StartsWith(idioma + " ", StringComparison.Ordinal));
    }

    private static bool EsCategoriaGramatical(string texto)
    {
        var categorias = new[]
        {
            "sustantivo", "verbo", "adjetivo", "adverbio", "interjeccion", "pronombre",
            "preposicion", "conjuncion", "determinante", "articulo", "participio",
            "locucion sustantiva", "locucion verbal", "locucion adjetiva", "locucion adverbial"
        };

        return categorias.Any(categoria =>
            texto == categoria || texto.StartsWith(categoria + " ", StringComparison.Ordinal));
    }

    private static bool EsEncabezadoDiccionario(string texto)
    {
        var normalizada = NormalizarClave(texto);
        var encabezados = new[]
        {
            "etimologia", "pronunciacion", "traducciones", "sinonimos", "antonimos",
            "referencias", "referencias y notas", "vease tambien", "locuciones", "refranes",
            "informacion adicional", "declinacion", "conjugacion", "derivados", "compuestos",
            "pronunciacion", "espanol", "ingles", "frances", "italiano", "portugues"
        };

        return encabezados.Any(encabezado =>
            normalizada == encabezado || normalizada.StartsWith(encabezado + " ", StringComparison.Ordinal));
    }

    private static string LimpiarDefinicion(string texto)
    {
        texto = Regex.Replace(texto, @"<[^>]+>", " ");
        texto = System.Net.WebUtility.HtmlDecode(texto);
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

        foreach (var original in contenido.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var linea = Regex.Replace(original, @"^\s*[#*•]+\s*", "").Trim();
            var coincidencia = Regex.Match(
                linea,
                @"(?:^|\s)Ejemplos?\s*:\s*(?<ejemplo>.+)$",
                RegexOptions.IgnoreCase);

            if (!coincidencia.Success)
                continue;

            var ejemplo = LimpiarDefinicion(coincidencia.Groups["ejemplo"].Value);
            if (ejemplo.Length > 5 && !ejemplos.Contains(ejemplo, StringComparer.OrdinalIgnoreCase))
                ejemplos.Add(ejemplo);
        }

        return ejemplos.Take(5).ToList();
    }
}
