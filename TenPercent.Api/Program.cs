using Microsoft.EntityFrameworkCore;
using TenPercent.Data;

var builder = WebApplication.CreateBuilder(args);

// --- НАСТРОЙКА НА БАЗАТА ДАННИ (MS SQL Server) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 1. НАСТРОЙКА НА CORS ---
// Позволяваме на React приложението (порт 5173) да прави заявки към това API
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Точният адрес на Vite фронтенда
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- 2. АКТИВИРАНЕ НА CORS (Много е важно да е точно тук, ПРЕДИ Authorization!) ---
app.UseCors("ReactAppPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();