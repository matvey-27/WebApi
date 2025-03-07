var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

DataBase.DataBase.CreateTable();



app.MapGet("/", () => "Hello World!");

app.Run();
