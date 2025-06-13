using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; // Necessário para IConfiguration

namespace UsersManager.Data
{
    // Esta classe é usada pelas ferramentas de design-time do Entity Framework Core
    // para criar uma instância do AppDbContext quando o projeto é construído
    // para operações como 'Add-Migration' ou 'Update-Database'.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // String de conexão inserida manualmente para a migração.
            // ATENÇÃO: Em um ambiente de produção, é altamente recomendável
            // ler a string de conexão de um arquivo de configuração (appsettings.json)
            // ou de variáveis de ambiente por questões de segurança e flexibilidade.
            string connectionString = "Data Source=usersmanager.db"; // Exemplo para SQLite

            // Se você estiver usando SQL Server, use:
            // string connectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";
            // optionsBuilder.UseSqlServer(connectionString);

            // Se você estiver usando PostgreSQL, use:
            // string connectionString = "Host=localhost;Port=5432;Database=myDataBase;Username=myUsername;Password=myPassword;";
            // optionsBuilder.UseNpgsql(connectionString);

            // Para SQLite, a linha abaixo é apropriada.
            optionsBuilder.UseSqlite(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
