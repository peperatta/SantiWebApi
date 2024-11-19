var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar CORS para permitir peticiones desde cualquier origen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

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

// Habilitar el uso de CORS
app.UseCors("AllowAll");

// Middleware para mostrar logs detallados de cada request
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request URL: {context.Request.Path}");
    await next.Invoke();
    Console.WriteLine($"Response Status: {context.Response.StatusCode}");
});

app.UseAuthorization();

app.MapControllers();

app.Run();
