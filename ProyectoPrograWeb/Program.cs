using ProyectoPrograWeb.services;

var builder = WebApplication.CreateBuilder(args);

// Agregar controladores al contenedor
builder.Services.AddControllers();

// Registrar el servicio de Firebase como singleton
// (una sola instancia para toda la aplicación)
builder.Services.AddSingleton<firebaseservice>();

// Registrar el servicio de solicitudes de donación
builder.Services.AddScoped<RequestService>();

// Documentación OpenAPI para probar los endpoints
builder.Services.AddOpenApi();

// Configurar CORS para permitir conexiones desde el frontend Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Logging para registrar errores y eventos
builder.Services.AddLogging();

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Activar CORS antes de los controladores
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();