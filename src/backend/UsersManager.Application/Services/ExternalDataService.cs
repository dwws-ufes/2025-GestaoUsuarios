using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UsersManager.Application.Services
{
    // Classe para representar um item do cache
    public class CacheItem
    {
        public string? Abstract { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ExternalDataService : IExternalDataService
    {
        private readonly HttpClient _httpClient;
        private readonly string _cacheFilePath;
        private Dictionary<string, CacheItem> _cache;

        // Configuração de validade do cache: 7 dias
        private const int _cacheValidityInDays = 7;

        public ExternalDataService()
        {
            _httpClient = new HttpClient();
            _cacheFilePath = "dbpedia_cache.json";
            _cache = LoadCache();
        }

        public async Task<string?> ObterDescricaoDbpedia(string termo, string idioma = "pt")
        {
            idioma = idioma.Split('-')[0].ToLower();
            string cacheKey = $"{termo}_{idioma}";

            // 1. Tentar obter do cache local e verificar a validade
            if (_cache.TryGetValue(cacheKey, out var cachedItem))
            {
                // Se o item do cache ainda for válido, retorne-o
                if ((DateTime.Now - cachedItem.LastUpdated).TotalDays < _cacheValidityInDays)
                {
                    return cachedItem.Abstract;
                }

                // Se o cache expirou, ele será ignorado e uma nova consulta será feita
            }

            // 2. Se não estiver no cache ou se expirou, tentar a busca no DBpedia
            string? descricao = null;
            try
            {
                string termoCapitalizado = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(termo.ToLower());

                descricao = await BuscarDescricaoPorUri(termoCapitalizado, idioma);

                if (string.IsNullOrEmpty(descricao))
                {
                    descricao = await BuscarDescricaoPorRegex(termo, idioma);
                }

                if (string.IsNullOrEmpty(descricao) && idioma != "en")
                {
                    descricao = await BuscarDescricaoPorRegex(termo, "en");
                }
            }
            catch (Exception)
            {
                // Em caso de falha na consulta externa, se o item expirado existe, retorne-o como fallback
                if (cachedItem != null)
                {
                    return cachedItem.Abstract;
                }
                // Senão, retorna nulo
                return null;
            }

            // 3. Se a busca externa for bem-sucedida, salva no cache e no arquivo
            if (!string.IsNullOrEmpty(descricao))
            {
                _cache[cacheKey] = new CacheItem { Abstract = descricao, LastUpdated = DateTime.Now };
                SaveCache();
            }

            return descricao;
        }

        private async Task<string?> BuscarDescricaoPorUri(string termo, string idioma)
        {
            string termoUri = termo.Replace(" ", "_").Trim();

            string sparql = $@"
                PREFIX dbo: <http://dbpedia.org/ontology/>
                PREFIX dbr: <http://dbpedia.org/resource/>

                SELECT ?abstract WHERE {{
                    dbr:{termoUri} dbo:abstract ?abstract .
                    FILTER (lang(?abstract) = '{idioma}')
                }} LIMIT 1
            ";

            return await ExecuteSparqlQuery(sparql);
        }

        private async Task<string?> BuscarDescricaoPorRegex(string termo, string idioma)
        {
            string sparql = $@"
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                PREFIX dbo: <http://dbpedia.org/ontology/>

                SELECT ?abstract WHERE {{
                    ?s rdfs:label ?label .
                    FILTER (lang(?label) = '{idioma}')
                    FILTER regex(?label, '{termo}', 'i') .
                    ?s dbo:abstract ?abstract .
                    FILTER (lang(?abstract) = '{idioma}')
                }} LIMIT 1
            ";

            return await ExecuteSparqlQuery(sparql);
        }

        private async Task<string?> ExecuteSparqlQuery(string sparql)
        {
            string endpoint = "https://dbpedia.org/sparql";
            var url = $"{endpoint}?query={Uri.EscapeDataString(sparql)}&format=application/sparql-results+xml";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(xml);
            var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "literal");

            return result?.Value;
        }

        private Dictionary<string, CacheItem> LoadCache()
        {
            if (File.Exists(_cacheFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(_cacheFilePath);
                    return JsonSerializer.Deserialize<Dictionary<string, CacheItem>>(jsonString)
                           ?? new Dictionary<string, CacheItem>();
                }
                catch
                {
                    return new Dictionary<string, CacheItem>();
                }
            }
            return new Dictionary<string, CacheItem>();
        }

        private void SaveCache()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_cache, options);
                File.WriteAllText(_cacheFilePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}