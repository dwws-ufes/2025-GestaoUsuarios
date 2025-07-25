namespace UsersManager.Application.Services
{
    public interface IExternalDataService
    {
        Task<string?> ObterDescricaoDbpedia(string termo, string idioma = "pt");
    }
}
