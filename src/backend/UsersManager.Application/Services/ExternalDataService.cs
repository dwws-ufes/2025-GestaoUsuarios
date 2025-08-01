using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UsersManager.Application.Services
{
    public class ExternalDataService : IExternalDataService
    {
        private readonly HttpClient _httpClient;

        public ExternalDataService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string?> ObterDescricaoDbpedia(string termo, string idioma = "pt")
        {
            idioma = idioma.Split('-')[0].ToLower();

            // Capitaliza o termo para a busca URI (ex: "teacher" -> "Teacher")
            string termoCapitalizado = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(termo.ToLower());

            // 1. Tentar a busca rápida pelo URI capitalizado.
            string? descricao = await BuscarDescricaoPorUri(termoCapitalizado, idioma);
            if (!string.IsNullOrEmpty(descricao))
            {
                return descricao;
            }

            // 2. Se a primeira tentativa falhar, tentar a busca mais robusta com REGEX (usando o termo original).
            descricao = await BuscarDescricaoPorRegex(termo, idioma);

            // 3. Se ainda não encontrar, tentar a busca com REGEX e fallback para o inglês.
            if (string.IsNullOrEmpty(descricao) && idioma != "en")
            {
                descricao = await BuscarDescricaoPorRegex(termo, "en");
            }

            return descricao;
        }


        private async Task<string?> BuscarDescricaoPorUri(string termo, string idioma)
        {
            // Limpa o termo e substitui espaços por underscore para o formato URI da DBpedia.
            string termoUri = termo.Replace(" ", "_").Trim();

            string sparql = $@"
                SELECT ?abstract WHERE {{
                    dbr:{termoUri} dbo:abstract ?abstract .
                    FILTER (lang(?abstract) = '{idioma}')
                }} LIMIT 1
            ";

            return await ExecuteSparqlQuery(sparql);
        }

        private async Task<string?> BuscarDescricaoPorRegex(string termo, string idioma)
        {
            // Consulta com REGEX para busca case-insensitive e por parte do nome.
            string sparql = $@"
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

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var xml = await response.Content.ReadAsStringAsync();

                // Parse XML e extrai o valor do <literal>
                var doc = XDocument.Parse(xml);
                var result = doc.Descendants()
                                .FirstOrDefault(e => e.Name.LocalName == "literal");

                return result?.Value;
            }
            catch (HttpRequestException)
            {
                // Trata erros de requisição HTTP (ex: 404 Not Found, 500 Internal Server Error)
                return null;
            }
            catch (Exception)
            {
                // Trata outros erros, como falha no parsing do XML
                return null;
            }
        }
    }
}