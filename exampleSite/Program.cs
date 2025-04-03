using Microsoft.AspNetCore.Builder;
using utility;
myInclude my = new myInclude();
var builder = WebApplication.CreateBuilder(args);
// 註冊 MemoryCache 作為 Session 的儲存機制
builder.Services.AddDistributedMemoryCache(); // ⬅️ 這行是關鍵
                                              // 顯示錯誤詳細資訊
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 分鐘沒動作就過期
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/API/test", () => "HelloWorld!");
//app.MapGet("/easymap", () => Results.Bytes(my.file_get_contents("App/easymap.html"), "text/html"));

app.MapGet("/Error/404", () =>
{
    return Results.Text("404 Not Found");
});
app.MapGet("/Error/500", () =>
{
    return Results.Text("500 Not Found");
});
// 註冊 myApiTesterApi
app.RegisterTestApi();

app.UseSession(); // 啟用 Session
// app.UseDeveloperExceptionPage(); // debug 模式才要開
app.UseExceptionHandler("/Error/500");
app.UseStatusCodePagesWithReExecute("/Error/404");
app.Run();
