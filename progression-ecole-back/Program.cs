using ProgressionEcole.Models;
using ProgressionEcole.Repositories;
using ProgressionEcole.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration des chemins de données
builder.Services.Configure<DataPathsConfig>(
    builder.Configuration.GetSection("DataPaths"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<EleveRepository>();
builder.Services.AddSingleton<CategorieRepository>();
builder.Services.AddSingleton<ActiviteRepository>();
builder.Services.AddSingleton<PeriodeRepository>();
builder.Services.AddSingleton<ActivitePersonnaliseeRepository>();
builder.Services.AddSingleton<GenerationService>();


        // Récupère les origines CORS depuis la configuration
        var localhostOrigins = builder.Configuration.GetSection("Cors:Localhost").Get<string[]>() ?? new[] { "http://localhost:4200" };
        var frontendOrigins = builder.Configuration.GetSection("Cors:Frontend").Get<string[]>() ?? new[] { "*" };

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Localhost",
                policy => policy.WithOrigins(localhostOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod());
            options.AddPolicy("Frontend",
                policy => policy.WithOrigins(frontendOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod());
        });

// Créer le répertoire de données s'il n'existe pas
var dataConfig = builder.Configuration.GetSection("DataPaths").Get<DataPathsConfig>() ?? new DataPathsConfig();
if (!Directory.Exists(dataConfig.DataDirectory))
{
    Directory.CreateDirectory(dataConfig.DataDirectory);
    
    // Copier les fichiers de données initiaux si ils n'existent pas
    var sourceDataPath = "Data";
    if (Directory.Exists(sourceDataPath) && dataConfig.DataDirectory != sourceDataPath)
    {
        foreach (var file in Directory.GetFiles(sourceDataPath, "*.json"))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(dataConfig.DataDirectory, fileName);
            if (!File.Exists(destFile))
            {
                File.Copy(file, destFile);
            }
        }
        
        // Copier aussi les templates Word s'ils existent
        foreach (var file in Directory.GetFiles(sourceDataPath, "*.docx"))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(dataConfig.DataDirectory, fileName);
            if (!File.Exists(destFile))
            {
                File.Copy(file, destFile);
            }
        }
    }
}

builder.Services.AddControllers(); // Ajout du support des controllers

var app = builder.Build();


// Active la bonne policy CORS selon l'environnement
if (app.Environment.IsDevelopment())
{
    app.UseCors("Localhost");
}
else
{
    app.UseCors("Frontend");
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Endpoint minimaliste pour le ping
app.MapGet("/api/ping", () => Results.Ok("pong"));

app.MapControllers(); // Ajout du mapping des controllers

app.Run();
