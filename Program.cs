DataBase.Data.CreateTable();

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы для контроллеров
builder.Services.AddControllers();

var app = builder.Build();

// Включаем маршрутизацию
app.UseRouting();

// Верхнеуровневая маршрутизация
app.MapControllers();

app.Run();