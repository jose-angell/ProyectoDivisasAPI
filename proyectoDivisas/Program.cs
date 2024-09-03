using proyectoDivisas.Models;
using proyectoDivisas.Repositories;

var builder = WebApplication.CreateBuilder(args);
// Configurar MongoSettings
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));

// Registrar MongoDBRepository como un servicio singleton
builder.Services.AddSingleton<MongoDBRepository>();

builder.Services.AddScoped<IAlertaDivisasCollection, AlertaDivisaCollection>();

builder.Services.AddHttpClient<ExternalApiDivisas>(client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.app");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
