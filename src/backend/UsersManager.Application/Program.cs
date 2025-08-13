using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UsersManager.Application.Services;
using UsersManager.Data;
using UsersManager.Data.Entities;
using UsersManager.Data.Repositories;
using VDS.RDF;
using VDS.RDF.Ontology;

var builder = WebApplication.CreateBuilder(args);

// --- JWT CONFIG ---
var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = jwtConfig["SecretKey"];
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey!)),
    };
});

// --- DEPENDÊNCIAS ---
// Configuração do DbContext com SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, AppDbContext>();
builder.Services.AddScoped<IRepository<Usuario>, UsuarioRepository>();
builder.Services.AddScoped<IRepository<Perfil>, PerfilRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddSingleton<IExternalDataService, ExternalDataService>();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

builder.Services.AddHttpContextAccessor();

// Adiciona os serviços da aplicação
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:9000", 
                           "https://localhost:9000", 
                           "http://localhost:9001", 
                           "https://localhost:9001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Descrição do esquema de segurança
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT: {seu token}"
    });

    // Aplicação do esquema a todas as operações
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Criação do escopo para usar os serviços
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<Usuario>>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(context, hasher);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no Seed do banco: {ex.Message}");
        throw;
    }
}

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Para pegar os dados do cliente
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// --- Rota para servir o arquivo OWL da ontologia ---
app.MapGet("/api/ontology/vocabulary", async (HttpContext httpContext, IConfiguration configuration) =>
{
    // O nome do arquivo OWL deve ser "UsersManager.owl" e estar na raiz do projeto.
    var systemURL = configuration["systemURL"];
    if (string.IsNullOrEmpty(systemURL))
    {
        Console.WriteLine("systemURL não configurado em appsettings.json. O RDF pode não ter URIs base.");
        systemURL = "http://localhost:5000"; // Fallback
    }
    var uri = new Uri($"{systemURL}/ontology/vocabulary#");

    FileInfo fi = new FileInfo("UsersManager.owl");
    FileInfo fiTemp = new FileInfo(Path.Combine(fi.Directory.FullName, Guid.NewGuid().ToString() + ".owl"));
    var content = File.ReadAllText(fi.FullName).Replace("http://www.meusite.com/UsersManager-vocabulario#", uri.AbsolutePath);
    if (!String.IsNullOrWhiteSpace(content))
    {
        httpContext.Response.ContentType = "application/xml";
        await httpContext.Response.WriteAsync(content);
    }
    else
    {
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsync("Ontology file not found.");
    }
});

app.MapControllers();

app.Run();
