var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ProgressionEcole.Repositories.EleveRepository>();
builder.Services.AddSingleton<ProgressionEcole.Repositories.CategorieRepository>();
builder.Services.AddSingleton<ProgressionEcole.Repositories.ActiviteRepository>();


        // Récupère les origines CORS depuis la configuration
        var localhostOrigins = builder.Configuration.GetSection("Cors:Localhost").Get<string[]>();
        var frontendOrigins = builder.Configuration.GetSection("Cors:Frontend").Get<string[]>();

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
