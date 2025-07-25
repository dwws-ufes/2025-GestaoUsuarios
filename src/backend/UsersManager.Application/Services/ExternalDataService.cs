using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;


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
            string sparql = $@"
            SELECT ?abstract WHERE {{
                dbr:{termo.Replace(" ", "_")} dbo:abstract ?abstract .
                FILTER (lang(?abstract) = '{idioma}')
            }} LIMIT 1
        ";

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
            catch
            {
                return null;
            }
        }
    }

}
