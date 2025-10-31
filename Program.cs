var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // 加入 MVC 支援
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 移除複雜的依賴注入設定，改用簡單的直接建立方式

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 聊天頁面: http://localhost:5000/Chat
// 首頁: http://localhost:5000/Home
// Swagger: http://localhost:5000/swagger

app.UseHttpsRedirection();
app.UseStaticFiles(); // 支援靜態檔案
app.UseRouting(); // 啟用路由

// 加入 MVC 路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// http://localhost:5000/swagger/index.html
// http://localhost:5000/weatherforecast

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
